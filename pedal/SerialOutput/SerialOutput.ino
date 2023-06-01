void setup() {
  Serial.begin(9600);
}

void loop() {
  for (int i = 1; i <= 100; i++) {
    Serial.print("Pedal 1 Position: ");
    Serial.println(i);
    delay(50);
  }

  delay(1000);
}
