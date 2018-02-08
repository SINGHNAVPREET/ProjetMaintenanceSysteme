﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

	public int wallDamage = 1;
	public int pointsPerCadeau = 1;
	public float restartLevelDelay = 1f;

	private Animator animator;
	private int cadeau;

	protected override void Start ()
	{
		
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator>();

		//Get the current food point total stored in GameManager.instance between levels.
		cadeau = GameManager.instance.playerCadeauPoints;

		//Call the Start function of the MovingObject base class.
		base.Start ();
	}
		
	//This function is called when the behaviour becomes disabled or inactive.
	private void OnDisable ()
	{
		//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
		GameManager.instance.playerCadeauPoints = cadeau;
	}

	private void Update ()
	{
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;

		int horizontal = 0;     //Used to store the horizontal move direction.
		int vertical = 0;       //Used to store the vertical move direction.


		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
		horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
		Debug.Log (horizontal);
		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int) (Input.GetAxisRaw ("Vertical"));
		Debug.Log (vertical);
		//Check if moving horizontally, if so set vertical to zero.
		if(horizontal != 0)
		{
			vertical = 0;
		}

		//Check if we have a non-zero value for horizontal or vertical
		if(horizontal != 0 || vertical != 0)
		{
			//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
			//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
			AttemptMove<wall> (horizontal, vertical);
		}
	}
	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		Debug.Log ("move");
		//Every time player moves, subtract from food points total.
		cadeau--;

		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		base.AttemptMove <T> (xDir, yDir);

		//Hit allows us to reference the result of the Linecast done in Move.
		RaycastHit2D hit;

		//If Move returns true, meaning Player was able to move into an empty space.
		if (Move (xDir, yDir, out hit)) 
		{
			//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
		}

		//Since the player has moved and lost food points, check if the game has ended.
		CheckIfGameOver ();

		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameManager.instance.playersTurn = false;
	}

	protected override void OnCantMove <T> (T component)
	{
		//Set hitWall to equal the component passed in as a parameter.
		wall hitWall = component as wall;

		//Call the DamageWall function of the Wall we are hitting.
		hitWall.DamageWall (wallDamage);

		//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
		animator.SetTrigger ("playerChop");
	}


	//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
	private void OnTriggerEnter2D (Collider2D other)
	{
		//Check if the tag of the trigger collided with is Exit.
		if(other.tag == "Exit")
		{
			//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
			Invoke ("Restart", restartLevelDelay);

			//Disable the player object since level is over.
			enabled = false;
		}

		//Check if the tag of the trigger collided with is Food.
		else if(other.tag == "Cadeau")
		{
			//Add pointsPerFood to the players current food total.
			cadeau += pointsPerCadeau;

			//Disable the food object the player collided with.
			other.gameObject.SetActive (false);
		}
			
	}


	//Restart reloads the scene when called.
	private void Restart ()
	{
		//Load the last scene loaded, in this case Main, the only scene in the game.
		SceneManager.LoadScene (0);
	}


	//LoseFood is called when an enemy attacks the player.
	//It takes a parameter loss which specifies how many points to lose.
	public void LoseCadeau (int loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("playerHit");

		//Subtract lost food points from the players total.
		cadeau -= loss;

		//Check to see if game has ended.
		CheckIfGameOver ();
	}


	//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
	private void CheckIfGameOver ()
	{
		//Check if food point total is less than or equal to zero.
		if (cadeau <= 0) 
		{

			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver ();
		}
	}

}
