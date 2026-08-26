 # ACount - personal finance tracking app
Author: Zoya Ivanova



## Problem
Personal finances are spread across multiple bank accounts, shares accounts etc. Bank apps provide only limited insightes and historic data. Also, moving funds between savings accounts loses information about money source: users cannot say own money from interest earned anymore. This app aims to help connect balances across accounts and keep historic data, as well as providing insights and planning instruments.

## Month One scope
- track accounts and transactions
- categorise spending
- set monthly budgets
- compare planned versus actual spending

## Later ideas
- Track personal banking account - amounts and spandage
- Set and track goals and budgets
- Track savings accounts toward goals
- Predict savings account income and taxes
- Compare budgets with actual spending
- Connect to Kiwi bank account with daily/hourly sync
- Categorise spending
- Mobile and web frontend


## Configuration
The required application options are listed in the `example.env`.

Configuration priority:
```
environment variables
	↓
User Secrets
	↓
appsettings.Development.json
	↓
appsettings.json
```

To initialise secrets use:
```sh
dotnet user-secrets -p src/PebbleJar.Api set "Akahu:AppIdToken" "..."
```


## Security notes


## Limitations
- The App is a single-user - purely for simplicity reason. 
  Multi-user support later requires associating accounts and tokens with a local user.
