# Social Coder

This project is geared towards building a social-media like experience for programmers!


If you're interested in contributing here's [Environment](Environment.md) setup instructions/suggestions.

Of course, no app is complete without a [Database](Database.md)!

## Scripts

Before starting the database via docker-compose we need to setup some environment variables.

There are a few parameters you can specify if you want... running it with no parameters will
use default values for our connection string! Which is fine...

[View Script](create-env.ps1)

Please note this will override previously set values if you don't respecify them!
Additionally, this script will automatically update your connection string.

```bash
./create-env.ps1

./create-env.ps1 -databasePath Path\To\Where\You\Want\To\Store\Db
```

Start database from [docker-compose](docker-compose.yml)
```bash
./start-db.ps1
```

Create https-certs. Depending on your environment your IDE might have already set this up for you.
Otherwise, this script will do the trick... hopefully

```bash
./create-https-certs.ps1
```