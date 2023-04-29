<style>
H1{color:Blue !important;}
H2{color:DarkOrange !important;}
p{color:Black !important;}
</style>

# Color AccelStepper Pedal Controller 



This code controls a set of pedals using AccelStepper library with Arduino Uno. The pedals can be moved to their back and front limits and can read the load cell values. The following are the features of this code:

Three pedals can be controlled independently.
The code uses AccelStepper library to control stepper motors.
The pins used for each pedal are defined at the beginning of the code.
Encoder library is used to read the pedal positions.
Load cell values can be read for each pedal.
Pedals can be homed by moving them to the back and front limit switches.
The pedal settings can be adjusted in the code, such as maximum speed, acceleration, force, spring constant, and steps per revolution.

# Installation
Download and install the Arduino IDE.
Connect the Arduino board to your computer.
Open the .ino file in the Arduino IDE.
Compile and upload the code to the board.
Connect the stepper motors, encoders, limit switches, and load cells to the appropriate pins on the board.

# Usage
Power on the board.
Press the pedal to move it forward.
Release the pedal to stop it.
The load cell value for each pedal is displayed in the serial monitor.
To home the pedals, run the homePedals() function. (currently N/D)
Adjust the pedal settings in the code if necessary.

# Credits
This code was created by [Aftershock].
The code uses the following libraries:

[AccelStepper](https://www.airspayce.com/mikem/arduino/AccelStepper/)

[Encoder](https://www.pjrc.com/teensy/td_libs_Encoder.html)
