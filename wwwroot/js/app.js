$(document).ready(function () {
    // Initialize Toastr options
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    // Handle form submission
    $('#crypto-form').on('submit', function (e) {
        e.preventDefault();

        const cryptoCode = $('#cryptoCode').val().trim().toUpperCase();
        const currencies = $('#currencies').val();

        if (!cryptoCode) {
            toastr.error('Please enter a cryptocurrency code.', 'Input Error');
            return;
        }

        if (!currencies || currencies.length === 0) {
            toastr.error('Please select at least one target currency.', 'Input Error');
            return;
        }

        // Build the query string
        const queryParams = new URLSearchParams();
        currencies.forEach(currency => {
            queryParams.append('currencies', currency);
        });

        // Construct the API URL
        const apiUrl = `/crypto/${cryptoCode}?${queryParams.toString()}`;

        // Show a loading indicator
        $('#quote-results').html('<p>Loading...</p>');

        // Make the API call
        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => { throw err; });
                }
                return response.json();
            })
            .then(data => {
                toastr.success('Cryptocurrency prices fetched successfully.', 'Success');
                displayResults(data);
            })
            .catch(error => {
                console.error('Error:', error);
                toastr.error(error.message || 'An error occurred while fetching data.', 'Error');
                $('#quote-results').html(`<p class="text-danger">${error.message || 'An error occurred while fetching data.'}</p>`);
            });
    });

    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/cryptohub") // Must match the hub mapping in Program.cs
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    // Start the connection
    connection.start().then(function () {
        console.log("Connected to SignalR hub.");
        toastr.info("Connected to real-time update service.", "Connected");
    }).catch(function (err) {
        console.error(err.toString());
        toastr.error("Failed to connect to real-time update service.", "Connection Error");
    });

    // Handle incoming crypto updates
    connection.on("ReceiveCryptoUpdate", function (cryptoCode, currency, price) {
        console.log(`Received update: ${cryptoCode} - ${currency}: ${price}`);

        // Find the table row corresponding to the currency
        const row = $(`#quote-results table tbody tr`).filter(function () {
            return $(this).find('td:first').text() === currency;
        });

        if (row.length > 0) {
            row.find('td:nth-child(2)').text(price !== null ? price.toLocaleString() : 'N/A');
            toastr.info(`Price updated for ${currency}: ${price !== null ? price.toLocaleString() : 'N/A'}`, 'Price Update');
        } else {
            // Optionally, add a new row if it doesn't exist
            const newRow = `
                <tr>
                    <td>${currency}</td>
                    <td>${price !== null ? price.toLocaleString() : 'N/A'}</td>
                    <td>—</td>
                </tr>
            `;
            $('#quote-results table tbody').append(newRow);
            toastr.info(`New currency added: ${currency} with price ${price !== null ? price.toLocaleString() : 'N/A'}`, 'New Currency');
        }
    });

    // Polling for live updates every 60 seconds
    setInterval(function () {
        const cryptoCode = $('#cryptoCode').val().trim().toUpperCase();
        const currencies = $('#currencies').val();

        if (!cryptoCode || !currencies || currencies.length === 0) {
            return;
        }

        const queryParams = new URLSearchParams();
        currencies.forEach(currency => {
            queryParams.append('currencies', currency);
        });

        const apiUrl = `/crypto/${cryptoCode}?${queryParams.toString()}`;

        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    return response.json().then(err => { throw err; });
                }
                return response.json();
            })
            .then(data => {
                displayResults(data);
                toastr.info('Cryptocurrency prices updated via polling.', 'Update');
            })
            .catch(error => {
                console.error('Error:', error);
                toastr.error(error.message || 'An error occurred while fetching data.', 'Error');
                $('#quote-results').html(`<p class="text-danger">${error.message || 'An error occurred while fetching data.'}</p>`);
            });
    }, 60000); // 60,000 milliseconds = 60 seconds


    // Function to display results in the table
    function displayResults(data) {
        if (!data || data.length === 0) {
            $('#quote-results').html('<p>No data available.</p>');
            return;
        }

        let html = `
            <table class="table table-hover table-bordered">
                <thead class="thead-dark">
                    <tr>
                        <th>Currency</th>
                        <th>Price</th>
                        <th>Error</th>
                    </tr>
                </thead>
                <tbody>
        `;

        data.forEach(item => {
            html += `
                <tr>
                    <td>${item.currency}</td>
                    <td class="${item.price !== null ? 'price' : ''}">${item.price !== null ? item.price.toLocaleString() : 'N/A'}</td>
                    <td class="${item.error ? 'error' : ''}">${item.error || '—'}</td>
                </tr>
            `;
        });

        html += `
                </tbody>
            </table>
        `;

        $('#quote-results').html(html);
    }

    // Dark Mode Toggle Functionality
    $('#dark-mode-toggle').on('click', function () {
        $('body').toggleClass('dark-mode');
        // Toggle button text
        if ($('body').hasClass('dark-mode')) {
            $(this).text('Light Mode');
            toastr.success('Dark mode enabled.', 'Theme Changed');
        } else {
            $(this).text('Dark Mode');
            toastr.success('Light mode enabled.', 'Theme Changed');
        }
    });

    // Preserve Dark Mode preference using localStorage
    if (localStorage.getItem('theme') === 'dark') {
        $('body').addClass('dark-mode');
        $('#dark-mode-toggle').text('Light Mode');
    }

    $('#dark-mode-toggle').on('click', function () {
        if ($('body').hasClass('dark-mode')) {
            localStorage.setItem('theme', 'light');
        } else {
            localStorage.setItem('theme', 'dark');
        }
    });
});
