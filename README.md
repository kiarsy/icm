# IC MARKET

##  How To run

### 1-run Migration
```bash
dotnet ef database update \                                                                                                                                                                                                                                              development 
    --project ICMarkets.Infrastructure \
    --startup-project ICMarkets.Api
```
### 2-Build Project
```bash
dotnet build
```

### 3-Run Project
```bash
dotnet ICMarkets.Api/bin/Debug/net8.0/ICMarkets.Api.dll
```

