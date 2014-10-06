MusicBrowser
============

Simple MusicBrowser that uses Web Components/Polymer on the frontend and Starcounter/Discogs.com on the backend

Created for the purpose of live coding with Web Components at [Meet.js Summit 2014](http://summit.meetjs.pl/2014/) and [WebCamp Zagreb 2014](http://2014.webcampzg.org/)

## Installation

### 1. Compile and run the app

### 2. Import the data

Download the **masters** data file from http://www.discogs.com/data/, e.g. http://www.discogs.com/data/discogs_20140901_masters.xml.gz

Ungzip it to `data\discogs_masters.xml`

The file contains about 700k music album definitions, including artists, cover imagages and YouTube video links.

Go to [http://localhost:8080/load-data](http://localhost:8080/load-data) to start the import. The whole import may take up to 3 hours

### 3. Run the app

Go to [http://localhost:8080/](http://localhost:8080/)

The working app looks like this:

![screenshot](https://cloud.githubusercontent.com/assets/566463/4526983/1c3cd65c-4d64-11e4-849a-b4218c0b5880.png)
