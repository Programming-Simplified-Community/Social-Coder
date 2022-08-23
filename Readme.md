# Social Coder

This project is geared towards building a social-media like experience for programmers!


If you're interested in contributing here's [Environment](Environment.md) setup instructions/suggestions.

Of course, no app is complete without a [Database](Database.md)!

## Scripts

Start database from [docker-compose](docker-compose.yml)
```bash
./start-db.ps1
```

Create https-certs. Depending on your environment your IDE might have already set this up for you.
Otherwise, this script will do the trick... hopefully

```bash
./create-https-certs.ps1
```