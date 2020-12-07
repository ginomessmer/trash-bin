# Trash Bin Server(less) for ShareX
An experimental custom uploader for [ShareX](https://getsharex.com/) based on Azure Functions.

> :warning: **Not finished yet by all means.  
> This is just an experimental project and the current state serves as a POC because I didn't have enough spare time yet.**

### Supports
- [x] URL shortener
- [x] Image
- [x] Text
- [x] File

### Deploy
TODO - but should work just like any other Azure Function deployment (it's not rocket science).

### Uploader Configuration
Import this snippet into your ShareX custom upload destinations list. Make sure to replace the placeholders.
```json
{
  "Version": "13.3.0",
  "Name": "Azure Functions Trash Bin",
  "DestinationType": "ImageUploader, TextUploader, FileUploader, URLShortener",
  "RequestMethod": "POST",
  "RequestURL": "<YOUR_AZURE_FUNCTION_ENDPOINT>.azurewebsites.net/api/upload",
  "Parameters": {
    "code": "<AZURE_FUNCTION_SECRET_KEY>"
  },
  "Body": "MultipartFormData",
  "Arguments": {
    "input": "$input$",
    "filename": "$filename$"
  },
  "FileFormName": "blob",
  "URL": "$header:Location$"
}
```

### Useful?
[Let me know](https://twitter.com/ginomessmer) and if you feel generous, feel free to [buy me a coffee](buymeacoffee.com/ginomessmer).  
<a href="https://www.buymeacoffee.com/ginomessmer" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-yellow.png" alt="Buy Me A Coffee" height="40px" ></a>

