# Feliz on Desktop with Photino (Highly Experimental)

A full stack F# application on desktop with the help of [Photino](https://www.tryphotino.io).

Phontino hosts a lightweight chromioum instance in a desktop window and this application embeds a full web application in it.

Using [Suave](https://github.com/SuaveIO/suave) as an extremely lightweight web server for the backend and [Feliz](https://github.com/Zaid-Ajaj/Feliz)/React on the frontend.

Of course with fully type-safe data transport between client and server using [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting)

![image](photino-feliz.gif)

## Dependencies
The dependencies need are the .NET SDK and Node.js

On Windows 10, the Edge Dev Channel version needs to be installed from [Microsoft Edge Insider](https://www.microsoftedgeinsider.com/en-us/download) to be able to view the browser

## Development

Before doing anything, start with installing npm dependencies into the client using `npm install`.

Then to start development mode with hot module reloading, run:
```bash
npm start
```
This will start the development server after compiling the project, once it is finished, navigate to http://localhost:8080 to view the application .

While the frontend is running, go the `./Desktop` and run the application in debug mode using
```
dotnet restore
dotnet run
```
This will do two things:
 - Runs a Suave backend in the background on port 5000 (API for the frontend)
 - Opens a Photino window navigating to localhost:8080 which is where the frontend is being hosted

## Work In Progress

 - Build automation for packaging and running in release mode
