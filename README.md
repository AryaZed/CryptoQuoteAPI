# CryptoQuote Dashboard

![CryptoQuote Banner](path-to-your-banner-image.png) <!-- Optional: Add a banner image -->

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Demo](#demo)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
- [Usage](#usage)
- [Real-Time Updates](#real-time-updates)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Introduction

**CryptoQuote Dashboard** is a responsive web application designed to provide real-time cryptocurrency price quotes in multiple currencies. Users can input a cryptocurrency code (e.g., BTC) and select target currencies (e.g., USD, EUR) to fetch the latest prices. The application leverages **SignalR** for real-time updates, ensuring users receive the most recent data without manual refreshes.

## Features

- **Real-Time Price Updates:** Receive live cryptocurrency price updates using SignalR.
- **Responsive Design:** Optimized for desktops, tablets, and mobile devices using Bootstrap.
- **User-Friendly Interface:** Intuitive form inputs and dynamic tables for displaying results.
- **Dark Mode:** Toggle between Light and Dark themes for a personalized experience.
- **Notifications:** Stylish Toastr notifications for success, error, and informational messages.
- **Error Handling:** Graceful handling of API errors and invalid inputs.
- **Automatic Polling:** Fetches updated prices every 60 seconds to ensure data freshness.
- **Comprehensive Testing:** Robust unit and integration tests using XUnit and FluentAssertions.
- **Docker Support:** Easily containerize the application for deployment.
- **Logging and Monitoring:** Implemented using Serilog for effective monitoring and debugging.

## Demo

![CryptoQuote Demo](path-to-demo-image.gif) <!-- Optional: Add a demo GIF or image -->

## Technologies Used

### Frontend

- **HTML5**
- **CSS3** (Bootstrap)
- **JavaScript** (jQuery)
- **Toastr** for notifications
- **SignalR** for real-time communication

### Backend

- **ASP.NET Core**
- **SignalR Hub**
- **CoinMarketCap API** for fetching cryptocurrency data
- **Polly** for resilience and transient-fault handling
- **Docker** for containerization
- **Serilog** for logging
- **In-Memory Caching**
- **FluentValidation** for input validation
- **Rate Limiting**
- **XUnit** for testing
- **FluentAssertions** for expressive assertions
- **Moq** for mocking dependencies
- **BackgroundService** for periodic tasks

## Getting Started

### Prerequisites

Before you begin, ensure you have met the following requirements:

- **.NET 8.0 SDK or later:** [Download here](https://dotnet.microsoft.com/download)
- **Node.js and npm:** [Download here](https://nodejs.org/)
- **CoinMarketCap API Key:** Sign up and obtain an API key from [CoinMarketCap](https://coinmarketcap.com/api/)
- **Git:** [Download here](https://git-scm.com/downloads)
- **Docker (Optional):** [Download here](https://www.docker.com/get-started)

### Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/AryaZed/CryptoQuote-Dashboard.git
   cd CryptoQuote-Dashboard
