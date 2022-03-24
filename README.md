# Accident_Data_Flagger

A window's service that downloads CSV files from an SFTP, take vehicles from a given CSV file and uploads said vehicle to a SQL Job Queue. 
The serial number of the vehicle is obtained from a separate SQL table - using said serial number, 
that vehicle is "flagged" automatically on the Geotab Database by the service.  Another Service
will pick up the the "flagged" vehicle and will send all of its unedited "raw" data from the time 
of the accident, compile that data into a pdf, and save that data into a shared path.  The Accident_Data_Flagger
Service will pick up that file the next time it runs, and will "unflag" that vehicle.  This service is run
every three hours, coincinding with the release of CSV Files.
