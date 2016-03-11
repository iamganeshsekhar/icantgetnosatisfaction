﻿using UnityEngine;
using System.Collections;

public class TrainController : MonoBehaviour {

	public float throttleSpeed = 0f;
	public float throttleIncrement = 0.05f;
	public float trackMoveIncrement = 0.5f;

	public int passengerCapacity = 1000;
	public int passengerFull = 500;

	public int currentTrack = 1;

	private bool isEmergencyStopping = false;
	private bool trackDirectionUp = false;
	private int lastStationId = 0;
	private bool wasStoppedAtStation = false;
	private float[] trackPositions;

	private int passengersToDisembark = 0;
	private float lastDisembarkment = 0;

	// Use this for initialization
	void Start () {
		this.passengerFull = StationsController.Instance.getInitialPassengers(this.passengerCapacity);
		trackPositions = new float[3] {75f, 0f, -75f};
	}
	
	// Update is called once per frame
	void Update () {

		passengerEmbarkment();
		moveTrain();

	}


	private void passengerEmbarkment () {

		bool isStoppedAtStation = false;
		int currentStationId = 1;


		if (throttleSpeed == 0f) {
			isStoppedAtStation = true;
		}

		currentStationId = Mathf.FloorToInt(this.transform.position.x / 500) + 6;

		if (isStoppedAtStation) {
			wasStoppedAtStation = true;

			float currentTime = Time.realtimeSinceStartup;

			if (currentStationId > lastStationId) {
				lastStationId = currentStationId;
				lastDisembarkment = currentTime;
				passengersToDisembark = Mathf.FloorToInt(passengerFull / 10);
			}

			StationsController.Instance.arriveAtStation(passengersToDisembark);

			// Get new passengers from the station
			passengerFull += StationsController.Instance.embarkPassenger();

			// Two seconds between passengers disembarking
			if ((currentTime - lastDisembarkment > 0.2f) && passengersToDisembark > 0) {
				lastDisembarkment = currentTime;
				passengerFull--;
				passengersToDisembark--;
				//Debug.Log (passengersToDisembark);
				StationsController.Instance.disembarkPassenger();
			}
				
		} else {

			if (wasStoppedAtStation) {
				wasStoppedAtStation = false;
				StationsController.Instance.departStation();
			}

		}

	}


	private void moveTrain () {

		CameraController cameraController = Camera.main.GetComponent<CameraController>();

		float currentDeceleration = ((Time.deltaTime * 1000) * throttleIncrement);

		if (Input.GetKey("right")) {
			// Accelerating after emergency stopping cancels the emergency stop
			isEmergencyStopping = false;
			throttleSpeed = throttleSpeed + throttleIncrement;
		}

		// Cant decelerate faster than an emergency stop
		if (Input.GetKey("left") && !isEmergencyStopping) {
			currentDeceleration = -throttleIncrement;
			throttleSpeed = throttleSpeed + currentDeceleration;
		}

		// Allow user to hit space once instead of requiring them to hold it down
		if (Input.GetKey("space") || isEmergencyStopping) {
			isEmergencyStopping = true;
			currentDeceleration = -(3 * throttleIncrement);
			throttleSpeed = throttleSpeed + currentDeceleration;
		}

		if (Input.GetKeyUp("up")) {
			if (currentTrack > 0) {
				trackDirectionUp = true;
				Debug.Log ("Moving Up");
				currentTrack--;
			}
		}

		if (Input.GetKeyUp("down")) {
			if (currentTrack < trackPositions.Length - 1) {
				trackDirectionUp = false;
				Debug.Log ("Moving Down");
				currentTrack++;
			}
		}

		if (this.transform.position.y < trackPositions[currentTrack]) {
			this.setTrainY(this.transform.position.y + ((Time.deltaTime * 1000) * trackMoveIncrement));
		} else if (this.transform.position.y > trackPositions[currentTrack]) {
			this.setTrainY(this.transform.position.y - ((Time.deltaTime * 1000) * trackMoveIncrement));
		}

		if (trackDirectionUp && this.transform.position.y > trackPositions[currentTrack]) {
			this.setTrainY(trackPositions[currentTrack]);
		} else if (!trackDirectionUp && trackPositions[currentTrack] > this.transform.position.y) {
			this.setTrainY(trackPositions[currentTrack]);
		}

		if (throttleSpeed < 0f) {
			throttleSpeed = 0f;
		}

		transform.position += new Vector3(throttleSpeed, 0f, 0f);

		float currentX = transform.position.x;

		if (throttleSpeed > 0f) {
			cameraController.moveCameraBasedOnTrainPos(currentX, throttleSpeed / throttleIncrement);
		}

		if (Camera.main.transform.position.x > currentX + 500f) {
			cameraController.setCameraX(currentX + 500f);
		} else if (Camera.main.transform.position.x < currentX - 200f) {
			cameraController.setCameraX(currentX - 200f);
		}

	}


	private void setTrainY (float pos) {
		this.transform.position = new Vector3(
			this.transform.position.x,
			pos,
			this.transform.position.z
		);
	}


}
