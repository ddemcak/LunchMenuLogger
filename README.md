# Lunch Menu Logger

Simple .net core application capable of:
- Downloading specific *.pdf* file.
- Parsing text from the file.
- Saving data to database.

## How to use ##
Build or download Release package and set proper parameters to *config.ini* dile.
Run the application and provide *config.ini* file as the only parameter.

### Example ###
```ini
[General]
LunchMenuURL=https://example.com/menu.pdf

[Database]
Server=127.0.0.1
User=root
Password=toor
Name=lunchmenus
```

