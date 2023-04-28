#include <AccelStepper.h>
#include <Joystick.h>

// Define the number of steps per revolution for stepper motors
#define STEPS_PER_REVOLUTION 200

// Define the analog input pins connected to the load cells
#define PEDAL1_PIN A0
#define PEDAL2_PIN A1
#define PEDAL3_PIN A2

// Define the maximum pressure that can be applied to the pedals (in lbs)
#define MAX_PRESSURE 100

// Define the range of values for the stepper motor movement
#define MIN_MOVEMENT 0
#define MAX_MOVEMENT 500

// Define the pins connected to the stepper motors
#define MOTOR1_PIN 4
#define MOTOR2_PIN 5
#define MOTOR3_PIN 6

// Define the pins connected to the pedal switches
#define PEDAL1_SWITCH_FRONT_PIN 7
#define PEDAL1_SWITCH_BACK_PIN 8
#define PEDAL2_SWITCH_FRONT_PIN 9
#define PEDAL2_SWITCH_BACK_PIN 10
#define PEDAL3_SWITCH_FRONT_PIN 11
#define PEDAL3_SWITCH_BACK_PIN 12

// Define the joystick axis and button numbers
#define JOYSTICK_X_AXIS 0
#define JOYSTICK_Y_AXIS 1
#define JOYSTICK_Z_AXIS 2
#define JOYSTICK_BUTTON1 3
#define JOYSTICK_BUTTON2 4
#define JOYSTICK_BUTTON3 5

// Create AccelStepper objects for each stepper motor
AccelStepper motor1(AccelStepper::DRIVER, MOTOR1_PIN);
AccelStepper motor2(AccelStepper::DRIVER, MOTOR2_PIN);
AccelStepper motor3(AccelStepper::DRIVER, MOTOR3_PIN);

// Create a Joystick object with 3 axes and 3 buttons
Joystick joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD, 3,
                  0,  // 3 axes, no buttons
                  true, true,
                  false,  // X and Y axes are analog, Z axis is digital
                  true, true, true);  // All buttons are momentary

// Define the current position of each pedal
int pedal1_position = 0;
int pedal2_position = 0;
int pedal3_position = 0;

// Define the switch state for each pedal
bool pedal1_switch_front_state = false;
bool pedal1_switch_back_state = false;
bool pedal2_switch_front_state = false;
bool pedal2_switch_back_state = false;
bool pedal3_switch_front_state = false;
bool pedal3_switch_back_state = false;

void setup() {
  // Set the maximum speed and acceleration of the stepper motors
  motor1.setMaxSpeed(1000);
  motor1.setAcceleration(100);
  motor2.setMaxSpeed(1000);
  motor2.setAcceleration(100);
  motor3.setMaxSpeed(1000);
  motor3.setAcceleration(100);

  // Set the pedal switch pins as inputs with pull-up resistors enabled
  pinMode(PEDAL1_SWITCH_FRONT_PIN, INPUT_PULLUP);
  pinMode(PEDAL1_SWITCH_BACK_PIN, INPUT_PULLUP);
  pinMode(PEDAL2_SWITCH_FRONT_PIN, INPUT_PULLUP);
  pinMode(PEDAL2_SWITCH_BACK_PIN, INPUT_PULLUP);
  pinMode(PEDAL3_SWITCH_FRONT_PIN, INPUT_PULLUP);
  pinMode(PEDAL3_SWITCH_BACK_PIN, INPUT_PULLUP);

  // Initialize the Joystick library
  joystick.begin();

  // Initialize the Serial Monitor
  Serial.begin(9600);
}

void loop() {
  // Read the analog input signals from the load cells
  int pedal1_reading = analogRead(PEDAL1_PIN);
  int pedal2_reading = analogRead(PEDAL2_PIN);
  int pedal3_reading = analogRead(PEDAL3_PIN);

  // Map the load cell readings to a value between MIN_MOVEMENT and MAX_MOVEMENT
  int pedal1_movement =
      map(pedal1_reading, 0, 1023, MIN_MOVEMENT, MAX_MOVEMENT);
  int pedal2_movement =
      map(pedal2_reading, 0, 1023, MIN_MOVEMENT, MAX_MOVEMENT);
  int pedal3_movement =
      map(pedal3_reading, 0, 1023, MIN_MOVEMENT, MAX_MOVEMENT);

  // Calculate the pressure applied to each pedal (in lbs)
  float pedal1_pressure = (pedal1_reading * MAX_PRESSURE) / 1023.0;
  float pedal2_pressure = (pedal2_reading * MAX_PRESSURE) / 1023.0;
  float pedal3_pressure = (pedal3_reading * MAX_PRESSURE) / 1023.0;

  // Move the stepper motors to the new positions
  motor1.moveTo(pedal1_movement);
  motor2.moveTo(pedal2_movement);
  motor3.moveTo(pedal3_movement);
  motor1.run();
  motor2.run();
  motor3.run();

  // Read the switch states for each pedal
  pedal1_switch_front_state = digitalRead(PEDAL1_SWITCH_FRONT_PIN) == LOW;
  pedal1_switch_back_state = digitalRead(PEDAL1_SWITCH_BACK_PIN) == LOW;
  pedal2_switch_front_state = digitalRead(PEDAL2_SWITCH_FRONT_PIN) == LOW;
  pedal2_switch_back_state = digitalRead(PEDAL2_SWITCH_BACK_PIN) == LOW;
  pedal3_switch_front_state = digitalRead(PEDAL3_SWITCH_FRONT_PIN) == LOW;
  pedal3_switch_back_state = digitalRead(PEDAL3_SWITCH_BACK_PIN) == LOW;

  // Update the pedal positions based on the switch states
  if (pedal1_switch_front_state && !pedal1_switch_back_state) {
    pedal1_position = MAX_MOVEMENT;
  } else if (!pedal1_switch_front_state && pedal1_switch_back_state) {
    pedal1_position = MIN_MOVEMENT;
  }
  if (pedal2_switch_front_state && !pedal2_switch_back_state) {
    pedal2_position = MAX_MOVEMENT;
  } else if (!pedal2_switch_front_state && pedal2_switch_back_state) {
    pedal2_position = MIN_MOVEMENT;
  }
  if (pedal3_switch_front_state && !pedal3_switch_back_state) {
    pedal3_position = MAX_MOVEMENT;
  } else if (!pedal3_switch_front_state && pedal3_switch_back_state) {
    pedal3_position = MIN_MOVEMENT;
  }

  // Send the pedal data to the serial monitor
  Serial.print("Pedal 1: ");
  Serial.print(pedal1_pressure);
  Serial.print(" lbs (");
  Serial.print(pedal1_position);
  Serial.println(")");
  Serial.print("Pedal 2: ");
  Serial.print(pedal2_pressure);
  Serial.print(" lbs (");
  Serial.print(pedal2_position);
  Serial.println(")");
  Serial.print("Pedal 3: ");
  Serial.print(pedal3_pressure);
  Serial.print(" lbs (");
  Serial.print(pedal3_position);
  Serial.println(")");

  // Update the joystick axes
  joystick.setAxis(JOYSTICK_X_AXIS, pedal1_position);
  joystick.setAxis(JOYSTICK_Y_AXIS, pedal2_position);
  joystick.setAxis(JOYSTICK_Z_AXIS, pedal3_position);

  // Send the joystick data
  joystick.sendState();

  // Print the pedal positions and pressures to the Serial Monitor
  Serial.print("Pedal 1 position: ");
  Serial.print(pedal1_position);
  Serial.print("\tPressure: ");
  Serial.print(pedal1_pressure);
  Serial.println(" lbs");

  Serial.print("Pedal 2 position: ");
  Serial.print(pedal2_position);
  Serial.print("\tPressure: ");
  Serial.print(pedal2_pressure);
  Serial.println(" lbs");

  Serial.print("Pedal 3 position: ");
  Serial.print(pedal3_position);
  Serial.print("\tPressure: ");
  Serial.print(pedal3_pressure);
  Serial.println(" lbs");

  // Add a delay to prevent excessive updates to the Joystick and Serial Monitor
  delay(10);
}
