using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace RacingSimPedalsReader
{
    public static class Program
    {

        private static ComboBox? comPortComboBox;
        private static Button? startButton;

        private static Button? saveButton;
        private static ComboBox? saveComboBox;

        private static TextBox? renameTextBox;
        private static Button? renameButton;

        private static Button addPointButton;
        private static Button removePointButton;

        private static GraphControl pedal1Graph;
        private static GraphControl pedal2Graph;
        private static GraphControl pedal3Graph;
        private static string saveFolderPath = "SaveFiles";
        private static List<string> savedConfigurations;

        private static List<PointF>[] graph;

        private static SerialPort? serialPort;
        private static TextBox? dataTextBox;

        private static int pedal1Position;
        private static int pedal2Position;
        private static int pedal3Position;

        private static List<PointF>? pedal1ResponseCurve;
        private static List<PointF>? pedal2ResponseCurve;
        private static List<PointF>? pedal3ResponseCurve;

        private static string savesFilePath = "saves.json";

        private static Form mainWindow;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mainWindow = new Form();
            mainWindow.Size = new Size(655, 325);

            comPortComboBox = new ComboBox();
            comPortComboBox.Location = new Point(10, 10);
            comPortComboBox.Width = 200;
            comPortComboBox.Text = "Choose a COM port";

            startButton = new Button();
            startButton.Location = new Point(220, 10);
            startButton.Text = "Start";

            comPortComboBox.Items.AddRange(SerialPort.GetPortNames());
            startButton.Click += StartButton_Click;

            pedal1Graph = new GraphControl();
            pedal1Graph.Location = new Point(10, 125);
            pedal1Graph.Size = new Size(200, 100);

            pedal2Graph = new GraphControl();
            pedal2Graph.Location = new Point(220, 125);
            pedal2Graph.Size = new Size(200, 100);

            pedal3Graph = new GraphControl();
            pedal3Graph.Location = new Point(430, 125);
            pedal3Graph.Size = new Size(200, 100);

            Button editButton = new Button();
            editButton.Location = new Point(10, 250);
            editButton.Text = "Edit Graph";
            editButton.Click += EditButton_Click;

            Button resetButton = new Button();
            resetButton.Location = new Point(100, 250);
            resetButton.Text = "Reset Graph";
            resetButton.Click += ResetButton_Click;

            Button addPointButton = new Button();
            addPointButton.Name = "addPointButton";
            addPointButton.Location = new Point(190, 250);
            addPointButton.Text = "Add Point";
            addPointButton.Click += AddPointButton_Click;

            Button removePointButton = new Button();
            removePointButton.Name = "removePointButton";
            removePointButton.Location = new Point(280, 250);
            removePointButton.Text = "Remove Last Point";
            removePointButton.Click += RemovePointButton_Click;

            /*
            Button toggleVisibilityButton = new Button();
            toggleVisibilityButton.Location = new Point(560, 70);
            toggleVisibilityButton.Text = "Toggle Visibility";
            toggleVisibilityButton.Click += ToggleVisibilityButton_Click;
            */

            saveButton = new Button();
            saveButton.Location = new Point(300, 10);
            saveButton.Text = "Save";
            saveButton.Click += SaveButton_Click;

            saveComboBox = new ComboBox();
            saveComboBox.Location = new Point(400, 10);
            saveComboBox.Width = 150;
            saveComboBox.SelectedIndexChanged += SaveComboBox_SelectedIndexChanged;
            saveComboBox.Items.Add("Choose a Save");  
            saveComboBox.SelectedIndex = 0;
            saveComboBox.DropDown += (sender, e) => saveComboBox.Items.Remove("Choose a Save");

            renameTextBox = new TextBox();
            renameTextBox.Location = new Point(400, 40);
            renameTextBox.Width = 150;
            renameTextBox.Text = "Rename Save";
            renameTextBox.Click += RenameTextBox_Click;

            renameButton = new Button();
            renameButton.Location = new Point(560, 40);
            renameButton.Text = "Rename";
            renameButton.Click += RenameButton_Click;

            Button deleteButton = new Button();
            deleteButton.Location = new Point(560, 10);
            deleteButton.Text = "Delete";
            deleteButton.Click += DeleteButton_Click;

            dataTextBox = new TextBox();
            dataTextBox.Location = new Point(10, 40);
            dataTextBox.Width = 200;
            dataTextBox.Height = 70;
            dataTextBox.Multiline = true;
            dataTextBox.ScrollBars = ScrollBars.Vertical;
            dataTextBox.ReadOnly = true;

            /*
            mainWindow.Controls.Add(toggleVisibilityButton);
            */

            mainWindow.Controls.Add(dataTextBox);
            mainWindow.Controls.Add(deleteButton);
            mainWindow.Controls.Add(renameTextBox);
            mainWindow.Controls.Add(renameButton);
            mainWindow.Controls.Add(saveButton);
            mainWindow.Controls.Add(saveComboBox);
            mainWindow.Controls.Add(comPortComboBox);
            mainWindow.Controls.Add(startButton);
            mainWindow.Controls.Add(pedal1Graph);
            mainWindow.Controls.Add(pedal2Graph);
            mainWindow.Controls.Add(pedal3Graph);
            mainWindow.Controls.Add(editButton);
            mainWindow.Controls.Add(resetButton);
            mainWindow.Controls.Add(addPointButton);
            mainWindow.Controls.Add(removePointButton);

           
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }

       
            LoadSavedConfigurations();

            Application.Run(mainWindow);
        }
        private static void RenameTextBox_Click(object sender, EventArgs e)
        {
            if (renameTextBox.Text == "Rename Save")
            {
                renameTextBox.Text = string.Empty;
            }
        }

        private static void StartButton_Click(object sender, EventArgs e)
        {
            string selectedPort = comPortComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedPort))
            {
                try
                {
                    serialPort = new SerialPort(selectedPort, 9600);
                    serialPort.DataReceived += SerialPort_DataReceived;
                    serialPort.Open();

                    comPortComboBox.Enabled = false;
                    startButton.Enabled = false;

                    Console.WriteLine("Serial port opened. Press any key to exit.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();

            if (!string.IsNullOrEmpty(data))
            {
                // Use the Invoke method to update the dataTextBox from the main thread
                dataTextBox.Invoke(new Action(() =>
                {
                    dataTextBox.AppendText("Received: " + data + Environment.NewLine);
                }));

                if (data.Contains("Pedal 1 Position:"))
                {
                    int position = int.Parse(data.Split(':')[1].Trim());
                    Console.WriteLine("Pedal 1 Position: " + position);
                    pedal1Position = position;
                }
                else if (data.Contains("Pedal 2 Position:"))
                {
                    int position = int.Parse(data.Split(':')[1].Trim());
                    Console.WriteLine("Pedal 2 Position: " + position);
                    pedal2Position = position;
                }
                else if (data.Contains("Pedal 3 Position:"))
                {
                    int position = int.Parse(data.Split(':')[1].Trim());
                    Console.WriteLine("Pedal 3 Position: " + position);
                    pedal3Position = position;
                }

                int[] positions = { pedal1Position, pedal2Position, pedal3Position };
                int[] adjustedPositions = ComparePositionToGraphs(positions, graph);
                Console.WriteLine("Adjusted Positions: " + string.Join(", ", adjustedPositions));

                // Send the adjusted positions back to the Teensy/Arduino over serial
                for (int i = 0; i < adjustedPositions.Length; i++)
                {
                    string adjustedPositionMessage = "Adjusted Position " + (i + 1) + ": " + adjustedPositions[i];
                    serialPort.WriteLine(adjustedPositionMessage);
                }

                UpdateGameInput(pedal1Position, pedal2Position, pedal3Position);
            }
        }

        private static int[] ComparePositionToGraphs(int[] positions, List<PointF>[] pedalGraphs)
        {
            int[] adjustedPositions = new int[positions.Length];

            for (int pedalIndex = 0; pedalIndex < positions.Length; pedalIndex++)
            {
                int position = positions[pedalIndex];
                List<PointF> pedalGraph = pedalGraphs[pedalIndex];

                // Find the two closest points on the graph
                PointF previousPoint = pedalGraph[0];
                PointF nextPoint = pedalGraph[pedalGraph.Count - 1];

                // Find the closest line segment
                for (int i = 0; i < pedalGraph.Count - 1; i++)
                {
                    PointF currentPoint = pedalGraph[i];
                    PointF nextPointCandidate = pedalGraph[i + 1];

                    if (currentPoint.X <= position && nextPointCandidate.X >= position)
                    {
                        previousPoint = currentPoint;
                        nextPoint = nextPointCandidate;
                        break;
                    }
                }

                // Calculate the slope and y-intercept of the line connecting the two closest points
                float slope = (nextPoint.Y - previousPoint.Y) / (nextPoint.X - previousPoint.X);
                float yIntercept = previousPoint.Y - slope * previousPoint.X;

                // Calculate the y-coordinate on the line at the given position
                float interpolatedForce = slope * position + yIntercept;

                // Adjust the position based on the interpolated force
                int adjustedPosition = (int)(position * interpolatedForce);

                adjustedPositions[pedalIndex] = adjustedPosition;
            }

            return adjustedPositions;
        }

        private static void UpdateGameInput(int pedal1Position, int pedal2Position, int pedal3Position)
        {
            float ffb1 = GetFFBValue(pedal1Position, pedal1ResponseCurve);
            float ffb2 = GetFFBValue(pedal2Position, pedal2ResponseCurve);
            float ffb3 = GetFFBValue(pedal3Position, pedal3ResponseCurve);

            ApplyForceFeedback(ffb1, ffb2, ffb3);
        }

        private static float GetFFBValue(int pedalPosition, List<PointF> responseCurve)
        {
            if (responseCurve == null || responseCurve.Count == 0)
                return 0f;

            float normalizedPosition = (float)pedalPosition / 100f;
            float ffbValue = 0f;

            for (int i = 1; i < responseCurve.Count; i++)
            {
                if (normalizedPosition <= responseCurve[i].X)
                {
                    float x0 = responseCurve[i - 1].X;
                    float y0 = responseCurve[i - 1].Y;
                    float x1 = responseCurve[i].X;
                    float y1 = responseCurve[i].Y;

                    ffbValue = Interpolate(x0, y0, x1, y1, normalizedPosition);
                    break;
                }
            }

            return ffbValue;
        }

        private static float Interpolate(float x0, float y0, float x1, float y1, float x)
        {
            float y = y0 + (x - x0) * ((y1 - y0) / (x1 - x0));
            return y;
        }

        private static void ApplyForceFeedback(float ffb1, float ffb2, float ffb3)
        {
            // Apply force feedback values to the game
            // Update the game's force feedback system with the calculated values
            // Use ffb1, ffb2, and ffb3 for the respective pedals
            // Adjust the force feedback parameters according to your game's API or engine
            // Example:
            // GameAPI.SetPedalForceFeedback(1, ffb1);
            // GameAPI.SetPedalForceFeedback(2, ffb2);
            // GameAPI.SetPedalForceFeedback(3, ffb3);
        }

        private static void EditButton_Click(object sender, EventArgs e)
        {
            // Open a dialog or form to allow the user to edit the graph points
            // Update the pedal response curves based on the user's input

            // Example code to update the response curves with hardcoded points
            pedal1ResponseCurve = new List<PointF>()
            {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
            };

            pedal2ResponseCurve = new List<PointF>()
            {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
            };

            pedal3ResponseCurve = new List<PointF>()
            {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
            };

          
            pedal1Graph.RefreshGraph(pedal1ResponseCurve);
            pedal2Graph.RefreshGraph(pedal2ResponseCurve);
            pedal3Graph.RefreshGraph(pedal3ResponseCurve);
        }

        private static void ResetButton_Click(object sender, EventArgs e)
        {
        
            pedal1ResponseCurve = new List<PointF>()
    {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
    };

            pedal2ResponseCurve = new List<PointF>()
    {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
    };

            pedal3ResponseCurve = new List<PointF>()
    {
                new PointF(0f, 0f),
                new PointF(0.125f, 0.125f),
                new PointF(0.25f, 0.25f),
                new PointF(0.375f, 0.375f),
                new PointF(0.5f, 0.5f)
    };

           
            pedal1Graph.RefreshGraph(pedal1ResponseCurve);
            pedal2Graph.RefreshGraph(pedal2ResponseCurve);
            pedal3Graph.RefreshGraph(pedal3ResponseCurve);
        }

        private static void AddPointButton_Click(object sender, EventArgs e)
        {

                if (pedal1Graph.Pedal1GraphActive == 1 && pedal2ResponseCurve != null)
                {

                    float newX1 = 0.75f;
                    float newY1 = 0.75f;
                    pedal2ResponseCurve.Add(new PointF(newX1, newY1));
                    pedal2Graph.RefreshGraph(pedal2ResponseCurve);
                }
                else if (pedal2Graph.Pedal2GraphActive == 1 && pedal1ResponseCurve != null)
                {

                    float newX2 = 0.75f;
                    float newY2 = 0.75f;
                    pedal1ResponseCurve.Add(new PointF(newX2, newY2));
                    pedal1Graph.RefreshGraph(pedal1ResponseCurve);
                }
                else if (pedal3Graph.Pedal3GraphActive == 1 && pedal3ResponseCurve != null)
                {

                    float newX3 = 0.75f;
                    float newY3 = 0.75f;
                    pedal3ResponseCurve.Add(new PointF(newX3, newY3));
                    pedal3Graph.RefreshGraph(pedal3ResponseCurve);
                }

          
                pedal1Graph.Pedal1GraphActive = 1;
                pedal2Graph.Pedal2GraphActive = 1;
                pedal3Graph.Pedal3GraphActive = 1;
            }

        private static void RemovePointButton_Click(object sender, EventArgs e)
        {

                if (pedal1Graph.Pedal1GraphActive == 1 && pedal1ResponseCurve != null)
                {
                    
                    if (pedal1ResponseCurve.Count > 1)
                    {
                        pedal2ResponseCurve.RemoveAt(pedal2ResponseCurve.Count - 1);
                        pedal2Graph.RefreshGraph(pedal2ResponseCurve);
                    }
                }
                else if (pedal2Graph.Pedal2GraphActive == 1 && pedal2ResponseCurve != null)
                {
                 
                    if (pedal2ResponseCurve.Count > 1)
                    {
                        pedal1ResponseCurve.RemoveAt(pedal1ResponseCurve.Count - 1);
                        pedal1Graph.RefreshGraph(pedal1ResponseCurve);
                    }
                }
                else if (pedal3Graph.Pedal3GraphActive == 1 && pedal3ResponseCurve != null)
                {
                  
                    if (pedal3ResponseCurve.Count > 1)
                    {
                        pedal3ResponseCurve.RemoveAt(pedal3ResponseCurve.Count - 1);
                        pedal3Graph.RefreshGraph(pedal3ResponseCurve);
                    }
                }

            
                pedal1Graph.Pedal1GraphActive = 1;
                pedal2Graph.Pedal2GraphActive = 1;
                pedal3Graph.Pedal3GraphActive = 1;
            }

        private static void SaveButton_Click(object sender, EventArgs e)
        {

            string saveName = "Save " + (saveComboBox.Items.Count + 1); 
            SaveConfiguration(saveName);
        }

        private static void SaveConfiguration(string saveName)
        {

           
            string json = GetResponseCurvesAsJson();
            string saveFilePath = Path.Combine(saveFolderPath, saveName + ".json");
            File.WriteAllText(saveFilePath, json);

      
            saveComboBox.Items.Add(saveName);
            saveComboBox.SelectedIndex = saveComboBox.Items.Count - 1;

           
            savedConfigurations.Add(saveName);
        }

        private static string GetResponseCurvesAsJson()
        {
           
            var responseCurves = new Dictionary<string, List<PointF>>();
            responseCurves["Pedal1"] = pedal1ResponseCurve;
            responseCurves["Pedal2"] = pedal2ResponseCurve;
            responseCurves["Pedal3"] = pedal3ResponseCurve;

 
            string json = JsonConvert.SerializeObject(responseCurves);

            return json;
        }

        private static void SaveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            string selectedSaveName = saveComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedSaveName))
            {
                LoadConfiguration(selectedSaveName);
            }
        }

        private static void LoadConfiguration(string saveName)
        {
            string saveFilePath = Path.Combine(saveFolderPath, saveName + ".json");
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                var responseCurves = JsonConvert.DeserializeObject<Dictionary<string, List<PointF>>>(json);
                if (responseCurves != null)
                {
                    if (responseCurves.TryGetValue("Pedal1", out var pedal1Curve))
                    {
                        pedal1ResponseCurve = pedal1Curve; 
                        pedal1Graph.RefreshGraph(pedal1ResponseCurve); 
                    }
                    if (responseCurves.TryGetValue("Pedal2", out var pedal2Curve))
                    {
                        pedal2ResponseCurve = pedal2Curve; 
                        pedal2Graph.RefreshGraph(pedal2ResponseCurve); 
                    }
                    if (responseCurves.TryGetValue("Pedal3", out var pedal3Curve))
                    {
                        pedal3ResponseCurve = pedal3Curve; 
                        pedal3Graph.RefreshGraph(pedal3ResponseCurve); 
                    }
                }
            }
        }

        private static void RenameButton_Click(object sender, EventArgs e)
        {
            string selectedSaveName = saveComboBox.SelectedItem as string;
            string newSaveName = renameTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(selectedSaveName) && !string.IsNullOrEmpty(newSaveName))
            {
                if (selectedSaveName != newSaveName)
                {
                  
                    string oldFilePath = Path.Combine(saveFolderPath, selectedSaveName + ".json");
                    string newFilePath = Path.Combine(saveFolderPath, newSaveName + ".json");
                    File.Move(oldFilePath, newFilePath);

                
                    int selectedIndex = saveComboBox.SelectedIndex;
                    saveComboBox.Items[selectedIndex] = newSaveName;
                }
            }
        }

        private static void DeletePictureBox_Click(object sender, EventArgs e)
        {
            string selectedSaveName = saveComboBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedSaveName))
            {
              
                string filePath = Path.Combine(saveFolderPath, selectedSaveName + ".json");
                File.Delete(filePath);

                
                saveComboBox.Items.Remove(selectedSaveName);
                saveComboBox.SelectedIndex = -1;

            
                renameTextBox.Text = string.Empty;
            }
        }

        private static void DeleteButton_Click(object sender, EventArgs e)
        {
            if (saveComboBox.SelectedItem != null)
            {
                string selectedSave = saveComboBox.SelectedItem.ToString();

             
                string saveFilePath = Path.Combine(saveFolderPath, selectedSave + ".json");
                File.Delete(saveFilePath);

                // Remove the save name from the drop-down menu
                saveComboBox.Items.Remove(selectedSave);

              
                saveComboBox.SelectedIndex = -1;
            }
        }

        private static void LoadSavedConfigurations()
        {
            // Retrieve all saved configuration files
            string[] saveFiles = Directory.GetFiles(saveFolderPath, "*.json");

         
            savedConfigurations = new List<string>();

            foreach (string saveFile in saveFiles)
            {
                string saveName = Path.GetFileNameWithoutExtension(saveFile);

              
                saveComboBox.Items.Add(saveName);

    
                savedConfigurations.Add(saveName);
            }
        }

        /*
        private static int buttonState = 0;
        
        private static void ToggleVisibilityButton_Click(object sender, EventArgs e)
        {
            buttonState = buttonState == 0 ? 1 : 0;

            Button toggleButton = (Button)sender;
            toggleButton.Text = buttonState.ToString();

            if (buttonState == 0)
            {
                mainWindow.Controls.Add(addPointButton);
                mainWindow.Controls.Remove(removePointButton);
            }
            else
            {
                mainWindow.Controls.Add(removePointButton);
                mainWindow.Controls.Remove(addPointButton);
            }
            mainWindow.Refresh();
        }
        */

    }
}