Jonas McGowan-Martin
CS4732 - Computer Animation
Final Project: It Is What You Make It!!

Source Files:
	- /Scripts: Contains all the scripts for the project, all in C#.
		- RedBoid.cs:
			This is the default script in the prefab, it controls the flocking behavior for each red boid. The scripts for the other colors of boids are basically the same thing.
		- BlueBoid.cs
		- GreenBoid.cs
		- SceneSwitcher.cs: Provides the function the UI needs to load the levels when a button is pushed.
		- Seek.cs: Contains the code to make a boid follow the seek steering behavior, seeking to the crosshair controlled by the user.
		- Flee.cs: Contains the code to make a boid follow the flee steering behavior, fleeing from the crosshair controlled by the user.
		- Pursue.cs: Contains the code to make a boid follow the pursue steering behavior, pursuing the other boid in the scene.
		- Evade.cs: Contains the code to make a boid follow the evade steering behavior, evading the other boid in the scene.
		- Wander.cs: Contains the code to make a boid wander around the scene using the offset circle method.

This project was implemented as a Unity project, so there are several assets made for it to work:
	- /Resources:
		Contains all the prefabs used, including one for each of the boids, the crosshair, walls, and the UI.
	- /Mats:
		There are several materials in this folder that serve to color the different scene objects.
	- Scene Files:
		Contains the levels that are set up for each steering behavior.

video link: https://youtu.be/tBunK-YPyAc