# SubMerge
This app is to merge two subtitles in one form and serve csv file, json data in first version. From V2 on, user will be able to visualize that merged subtitles. With v3, authorization will join the game and users will be able to bookmark their subtitle partials and manage merged files. 

Right now, we are at the first version without a UI

3 different ways have been implemented as follows,

## Console Applicaiton
- Read files to be merged from a local path. Write output file to a file path

## Azrue function
- A function accepting two files to be merged and returns a merged file

### API (Minimal API wiht .Net 6)
##### (Basicly, this is api version of v2 which will be implemented as Azure Function)

- Current version is only using Azure cloud services 
- Accept blob names of the files stored on Azure Blog Storage
- Process files 
- Save merged file to Azure Blog Storage
- Save JSON data of processed data to CosmosDB (Core API)

### Integration Tests to be written
### Unit Tests are written with a minimum covarage

##To Run the Application

### SubMerge.Func
- You will need following settings on your configuration (local.settings.json can be used on local runs)
"CosmosDbConnectionString": "*************",
"AzureStorageConnectionString": "**************"
  


