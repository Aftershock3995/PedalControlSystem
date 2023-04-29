#include <AccelStepper.h>
#include <Encoder.h>
#include <FastAccelStepper.h>

// Pedal_1 settings
#define PEDAL1_STEPS_PER_REV 200
#define PEDAL1_MAX_SPEED 1000
#define PEDAL1_MAX_ACCEL 500
#define PEDAL1_MAX_FORCE 1000
#define PEDAL1_SPRING_CONSTANT 100

// Pedal_1_Pin assignments
#define PEDAL_1_STEP_PIN 2
#define PEDAL_1_DIR_PIN 3
#define PEDAL_1_ENC_A_PIN 4
#define PEDAL_1_ENC_B_PIN 5
#define PEDAL_1_LIMIT_SWITCH_BACK_PIN 6
#define PEDAL_1_LIMIT_SWITCH_FRONT_PIN 7
#define PEDAL_1_LOAD_CELL_PIN A0

// Pedal_2 settings
#define PEDAL2_STEPS_PER_REV 200
#define PEDAL2_MAX_SPEED 1000
#define PEDAL2_MAX_ACCEL 500
#define PEDAL2_MAX_FORCE 1000
#define PEDAL2_SPRING_CONSTANT 100

// Pedal_2_Pin assignments
#define PEDAL_2_STEP_PIN 8
#define PEDAL_2_DIR_PIN 9
#define PEDAL_2_ENC_A_PIN 10
#define PEDAL_2_ENC_B_PIN 11
#define PEDAL_2_LIMIT_SWITCH_BACK_PIN 12
#define PEDAL_2_LIMIT_SWITCH_FRONT_PIN 13
#define PEDAL_2_LOAD_CELL_PIN A1

// Pedal_3 settings
#define PEDAL3_STEPS_PER_REV 200
#define PEDAL3_MAX_SPEED 1000
#define PEDAL3_MAX_ACCEL 500
#define PEDAL3_MAX_FORCE 1000
#define PEDAL3_SPRING_CONSTANT 100

// Pedal_3_Pin assignments
#define PEDAL_3_STEP_PIN A2
#define PEDAL_3_DIR_PIN A3
#define PEDAL_3_ENC_A_PIN A4
#define PEDAL_3_ENC_B_PIN A5
#define PEDAL_3_LIMIT_SWITCH_BACK_PIN A6
#define PEDAL_3_LIMIT_SWITCH_FRONT_PIN A7
#define PEDAL_3_LOAD_CELL_PIN A8

// Create AccelStepper objects for each pedal

AccelStepper pedal1(AccelStepper::DRIVER, PEDAL_1_STEP_PIN, PEDAL_1_DIR_PIN);
AccelStepper pedal2(AccelStepper::DRIVER, PEDAL_2_STEP_PIN, PEDAL_2_DIR_PIN);
AccelStepper pedal3(AccelStepper::DRIVER, PEDAL_3_STEP_PIN, PEDAL_3_DIR_PIN);

// Create Encoder objects for each pedal

Encoder pedal1Encoder(PEDAL_1_ENC_A_PIN, PEDAL_1_ENC_B_PIN);
Encoder pedal2Encoder(PEDAL_2_ENC_A_PIN, PEDAL_2_ENC_B_PIN);
Encoder pedal3Encoder(PEDAL_3_ENC_A_PIN, PEDAL_3_ENC_B_PIN);

// Create variables to store the current pedal positions

long pedal1Position = 0;
long pedal2Position = 0;
long pedal3Position = 0;

// Create variables to store the current load cell values

int pedal1LoadCellValue = 0;
int pedal2LoadCellValue = 0;
int pedal3LoadCellValue = 0;

// Function to home the pedals by moving them to the back and front limit switches

void setup()
{
  // Initialize serial communication

  Serial.begin(9600);

  // Home each pedal one at a time idk looks cooler

  Serial.println("Homing pedal 1...");
  homePedal1();
  Serial.println("Homing pedal 2...");
  homePedal2();
  Serial.println("Homing pedal 3...");
  homePedal3();

  // Initialize limit switches as inputs

  pinMode(PEDAL_1_LIMIT_SWITCH_BACK_PIN, INPUT_PULLUP);
  pinMode(PEDAL_1_LIMIT_SWITCH_FRONT_PIN, INPUT_PULLUP);
  pinMode(PEDAL_2_LIMIT_SWITCH_BACK_PIN, INPUT_PULLUP);
  pinMode(PEDAL_2_LIMIT_SWITCH_FRONT_PIN, INPUT_PULLUP);
  pinMode(PEDAL_3_LIMIT_SWITCH_BACK_PIN, INPUT_PULLUP);
  pinMode(PEDAL_3_LIMIT_SWITCH_FRONT_PIN, INPUT_PULLUP);

  // Set maximum speed and acceleration

  pedal1.setMaxSpeed(PEDAL1_MAX_SPEED);
  pedal2.setMaxSpeed(PEDAL2_MAX_SPEED);
  pedal3.setMaxSpeed(PEDAL3_MAX_SPEED);
}

void homePedal1()
{
  // Home pedal 1

  pedal1.setSpeed(-PEDAL1_MAX_SPEED);
  while (digitalRead(PEDAL_1_LIMIT_SWITCH_BACK_PIN) == HIGH)
  {
    pedal1.runSpeed();
  }
  pedal1.setCurrentPosition(0);
  pedal1.setSpeed(PEDAL1_MAX_SPEED);
  while (digitalRead(PEDAL_1_LIMIT_SWITCH_FRONT_PIN) == HIGH)
  {
    pedal1.runSpeed();
  }
  pedal1.setCurrentPosition(pedal1.targetPosition());
}

void homePedal2()
{
  // Home pedal 2

  pedal2.setSpeed(-PEDAL2_MAX_SPEED);
  while (digitalRead(PEDAL_2_LIMIT_SWITCH_BACK_PIN) == HIGH)
  {
    pedal2.runSpeed();
  }
  pedal2.setCurrentPosition(0);
  pedal1.setSpeed(PEDAL2_MAX_SPEED);
  while (digitalRead(PEDAL_2_LIMIT_SWITCH_FRONT_PIN) == HIGH)
  {
    pedal2.runSpeed();
  }
  pedal2.setCurrentPosition(pedal2.targetPosition());
  pedal2.moveTo(pedal2.currentPosition() - PEDAL2_STEPS_PER_REV);
  pedal2.runToPosition();
}

void homePedal3()
{
  // Home pedal 3

  pedal3.setSpeed(-1000);
  while (digitalRead(PEDAL_3_LIMIT_SWITCH_BACK_PIN) == HIGH)
  {
    pedal3.runSpeed();
  }
  pedal3.setCurrentPosition(0);
  pedal1.setSpeed(PEDAL3_MAX_SPEED);
  while (digitalRead(PEDAL_3_LIMIT_SWITCH_FRONT_PIN) == HIGH)
  {
    pedal3.runSpeed();
  }
  pedal3.setCurrentPosition(pedal3.targetPosition());
  pedal3.moveTo(pedal3.currentPosition() - PEDAL3_STEPS_PER_REV);
  pedal3.runToPosition();
}

int readLoadCellValue(int loadCellPin)
{
  return analogRead(loadCellPin);
}

void loop()
{
  // Read the load cell values for each pedal

  pedal1LoadCellValue = readLoadCellValue(PEDAL_1_LOAD_CELL_PIN);
  pedal2LoadCellValue = readLoadCellValue(PEDAL_2_LOAD_CELL_PIN);
  pedal3LoadCellValue = readLoadCellValue(PEDAL_3_LOAD_CELL_PIN);

  // Calculate the force to apply to each pedal based on the load cell value

  int pedal1Force = (pedal1LoadCellValue * PEDAL1_SPRING_CONSTANT) / 1024;
  int pedal2Force = (pedal2LoadCellValue * PEDAL2_SPRING_CONSTANT) / 1024;
  int pedal3Force = (pedal3LoadCellValue * PEDAL3_SPRING_CONSTANT) / 1024;

  // Move each pedal to the desired position based on the calculated force

  pedal1.moveTo(pedal1Position - (pedal1Force / 2));
  pedal2.moveTo(pedal2Position - (pedal2Force / 2));
  pedal3.moveTo(pedal3Position - (pedal3Force / 2));

  // Update the current pedal positions

  pedal1Position = pedal1.currentPosition();
  pedal2Position = pedal2.currentPosition();
  pedal3Position = pedal3.currentPosition();

  // Run the stepper motors

  pedal1.run();
  pedal2.run();
  pedal3.run();

  // Move each pedal back to its home position if the load cell value drops to zero

  if (pedal1LoadCellValue == 0)
  {
    pedal1.moveTo(0);
  }
  if (pedal2LoadCellValue == 0)
  {
    pedal2.moveTo(0);
  }
  if (pedal3LoadCellValue == 0)
  {
    pedal3.moveTo(0);
  }
}
