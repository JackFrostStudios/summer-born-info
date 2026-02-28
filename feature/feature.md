# Feature #
The API implements a test schools endpoint but now we must implement the real models and API.
We need an API that will accept a CSV file upload containing an export of UK school data and import the data into the database.
We also need an API that will return a list of all schools so we can check the import has been successful.

## Technical Details ##
The current School entity and APIs can be removed and replaced with the actual implementations required by this feature.
### Import API ###
Create a single "Import Schools" API should import the CSV file, and map each row to a set of entities.
The main "school" entity will be unique across the file, but the "linked entities" could be duplicated throughout the CSV.
For example each school will have a LA (local authority), there might be multiple rows with the same LA, this should be handled by detecting the duplicate based on the unique ID in the file and only processing one time.
It is save to assume that if the unique ID of that entity is matching we can assume the first row will have the same "details" (e.g. name) as all other rows.
When saving the record to the database, it should be treated as an "upsert" so if the entity already exists it's details are updated to match the new values.

### Schools API ###
Create a single "Get All Schools" API that returns a paginated list of schools.
Return schools in batches of 100.
Use cursor based pagination using the School ID.

## Suggested entities ##
Not all columns on the CSV are required, some can be ignored, please look in the [suggested entities](./suggested_entities/) folder for the suggested entity structure.

## Example upload ##
Please use the [example csv file](./example_upload.csv) as the expected payload for the API.