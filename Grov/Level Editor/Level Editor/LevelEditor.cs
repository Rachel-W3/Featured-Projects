using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

/*
 * Color values for tile types
 * Floor : -6695580
 * Border Wall : -14513374
 * Water : -16776961
 * Rocks : -4144960
 * Door : -8388480
 * Log : -11123140
 * Bridge : -128
 */

namespace Level_Editor
{
    public partial class LevelEditor : Form
    {
        private int width;
        private int height;
        private int boxSize;
        private Color currentColor;
        private Image currentImage;
        private PictureBox[,] mapArray;

        private int pallette;
        private int palletteMax = 1;

        //Is the title of the window == the title of the file we have open?
        private bool fileNameTitle;
        //Are there unsaved changes?
        private bool unsavedChanges;
        //Name of the file we're working on
        //Also the path/directory of the file
        private string fileName;

        //This constructor is for making a new file, NOT loading one
        //This is called from the previous window, never once we're already in the editor screen
        public LevelEditor(int height, int width)
        {
            this.width = width;
            this.height = height;

            fileNameTitle = false;
            unsavedChanges = false;

            this.pallette = 0;
            
            //Default Color is blue
            currentColor = Color.FromArgb(153, 213, 100);
            currentImage = null;

            //Close event
            this.FormClosing += CloseEvent;

            InitializeComponent();

            //Create a default map
            CreateMap();
        }
        //This constructor is for loading existing files
        //This is called from the previous window, never once we're already in the editor screen
        public LevelEditor(string fileName)
        {
            InitializeComponent();

            //Close event
            this.FormClosing += CloseEvent;

            this.fileName = fileName;
            //open the chosen file
            LoadMap(fileName);
        }



        /// <summary>
        /// Change the color of the brush
        /// </summary>
        private void colorButton_Click(object sender, EventArgs e)
        {
            currentColor = ((Button)sender).BackColor;
            currentImage = null;
            currentTilePicture.BackColor = currentColor;
            currentTilePicture.Image = null;
            Console.WriteLine(((Button)sender).BackColor.ToArgb());
            //153, 213, 100
        }
        /// <summary>
        /// Change the color of the selected map tile
        /// </summary>
        private void MapSquare_Click(object sender, EventArgs e)
        {
            //Un-capture the mouse
            ((PictureBox)sender).Capture = false;

            if (MouseButtons == MouseButtons.Left)
            {
                //Change the color
                ((PictureBox)sender).BackColor = currentColor;
                ((PictureBox)sender).Image = currentImage;

                //There are now unsaved changes
                if (fileNameTitle)
                {
                    this.Text = string.Format("Level Editor - {0} *", fileName);
                }
                else
                {
                    this.Text = "Level Editor *";
                }
                unsavedChanges = true;
            }
        }
        //When you click on an enemy
        private void enemy_Click(object sender, EventArgs e)
        {
            if(sender == eraseEnemy)
            {
                currentImage = null;
                currentTilePicture.Image = null;
            }
            else
            {
                currentImage = ((PictureBox)sender).Image;
                currentTilePicture.Image = ((PictureBox)sender).Image;
            }
        }



        /// <summary>
        /// Save the map to a file
        /// </summary>
        private void saveButton_Click(object sender, EventArgs e)
        {
            //Open a save dialog
            SaveFileDialog saveDialog = new SaveFileDialog();
            //Misc. visual flair
            saveDialog.Title = "Save a Level File";
            saveDialog.Filter = "Level Files (*.grovlev)|*.grovlev";
            saveDialog.FileName = "MyLevel";
            DialogResult result = saveDialog.ShowDialog();
            
            //Code only executes if they chose to go ahead and save the file
            if(result == DialogResult.OK)
            {
                BinaryWriter writer = null;

                try
                {
                    Stream streamOut = File.OpenWrite(saveDialog.FileName);
                    writer = new BinaryWriter(streamOut);

                    //Write the RGB value of every tile in order
                    for(int i = 0; i < height; i++)
                    {
                        for(int ii = 0; ii < width; ii++)
                        {
                            //Write the int corresponding to the enum
                            switch(mapArray[i, ii].BackColor.ToArgb())
                                {
                                    //Floor
                                    case -6695580:
                                        writer.Write(0);
                                        break;
                                    //Border wall
                                    case -14513374:
                                        writer.Write(1);
                                        break;
                                    //Water
                                    case -16776961:
                                        writer.Write(3);
                                        break;
                                    //Rocks
                                    case -4144960:
                                        writer.Write(4);
                                        break;
                                    //Door
                                    case -8388480:
                                        writer.Write(2);
                                        break;
                                    //Log
                                    case -11123140:
                                        writer.Write(5);
                                        break;
                                    //Bridge
                                    case -128:
                                        writer.Write(6);
                                        break;
                                    //Flower
                                    case -65281:
                                        writer.Write(7);
                                        break;
                                    //Boss door
                                    case -65536:
                                        writer.Write(8);
                                        break;
                                    //HELP ME :'(
                                    default:
                                        throw new Exception();
                                        break;
                                }
                        }
                    }

                    //Get the number of enemies
                    int enemyCount = 0;
                    for(int i = 0; i < height; i++)
                    {
                        for (int ii = 0; ii < width; ii++)
                        {
                            if (mapArray[i, ii].Image != null)
                            {
                                enemyCount++;
                            }
                        }
                    }

                    //Confirm number of enemies
                    writer.Write(enemyCount);

                    //Coordinates and enemy type for every enemy
                    for (int i = 0; i < height; i++)
                    {
                        for (int ii = 0; ii < width; ii++)
                        {
                            if (mapArray[i, ii].Image != null)
                            {
                                writer.Write(ii);
                                writer.Write(i);

                                //Enemy 0
                                if(mapArray[i, ii].Image == enemy0.Image)
                                {
                                    writer.Write(0);
                                }
                                //Enemy 1
                                else if (mapArray[i, ii].Image == enemy1.Image)
                                {
                                    writer.Write(1);
                                }
                                //Enemy 2
                                else if (mapArray[i, ii].Image == enemy2.Image)
                                {
                                    writer.Write(2);
                                }
                                //Enemy -1
                                else if(mapArray[i, ii].Image == enemyMinus1.Image)
                                {
                                    writer.Write(-1);
                                }
                                //Enemy -2
                                else if(mapArray[i, ii].Image == enemyMinus2.Image)
                                {
                                    writer.Write(-2);
                                }
                            }
                        }
                    }

                    //Message displays if it didn't break
                    MessageBox.Show("File Saved Successfully", "File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //Mark the file as saved
                    fileNameTitle = true;
                    unsavedChanges = false;
                    this.Text = string.Format("Level Editor - {0}", saveDialog.FileName);
                    fileName = saveDialog.FileName;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Format("Error writing to file: ", exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
            }
        }


        /// <summary>
        /// Execute when clicking the "load file" button
        /// </summary>
        private void loadButton_Click(object sender, EventArgs e)
        {
            //Throw a fit if there are unsaved changes
            if (unsavedChanges)
            {
                DialogResult panic = MessageBox.Show("You have unsaved changes! Are you sure you want to continue?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (panic != DialogResult.Yes)
                {
                    return;
                }
            }

            //Windows file selection dialog
            OpenFileDialog openFile = new OpenFileDialog();
            //Misc. visual flair
            openFile.Title = "Open a Level File";
            openFile.Filter = "Level Files (*.grovlev)|*.grovlev";
            DialogResult result = openFile.ShowDialog();
            if(result == DialogResult.OK)
            {
                LoadMap(openFile.FileName);
            }
        }


        /// <summary>
        /// Generates a grid from the given height and width
        /// </summary>
        private void CreateMap()
        {
            //Create a 2D array the size of the height
            mapBox.Controls.Clear();
            mapArray = new PictureBox[height, width];
            //Create the size of the box; equal to the height of the map / the number of map slots
            boxSize = (mapBox.Height - 27) / height;

            //Create a bunch of picture boxes, and assign all their values
            for (int i = 0; i < height; i++)
            {
                for (int ii = 0; ii < width; ii++)
                {
                    //i = height ii = width

                    mapArray[i, ii] = new PictureBox();
                    //Add it to the Map Box
                    mapBox.Controls.Add(mapArray[i, ii]);
                    //Default color
                    mapArray[i, ii].BackColor = Color.FromArgb(153, 213, 100);
                    mapArray[i, ii].SizeMode = PictureBoxSizeMode.StretchImage;
                    //Draw the borders
                    if (i == 0 || ii == 0 || i == 17 || ii == 31)
                    {
                        mapArray[i, ii].BackColor = Color.ForestGreen;
                    }
                    //Draw the doors
                    if (ii == 0 && i == 8) // Left door 1
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 0 && i == 9) // Left door 2
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 31 && i == 8) // Right door 1
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 31 && i == 9) // Right door 2
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 15 && i == 0) // Top door 1
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 16 && i == 0) // Top door 2
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 15 && i == 17) // Bottom door 1
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    if (ii == 16 && i == 17) // Bottom door 2
                    {
                        mapArray[i, ii].BackColor = Color.Purple;
                    }
                    //Set the size of the box
                    mapArray[i, ii].Height = boxSize;
                    mapArray[i, ii].Width = boxSize;
                    //Set the location
                    mapArray[i, ii].Location = new Point(10 + ii * boxSize, 18 + i * boxSize);

                    //Give it a click event
                    mapArray[i, ii].MouseDown += MapSquare_Click;
                    //Also give it a mouseenter event
                    mapArray[i, ii].MouseEnter += MapSquare_Click;
                }
            }

            //fix the size of the map box and window
            mapBox.Width = (boxSize * width) + 20;
            this.Width = (boxSize * width) + 268;
        }






        /// <summary>
        /// Once a file has been selected, load a map from it
        /// </summary>
        /// <param name="openFile">Path to the file</param>
        private void LoadMap(string openFile)
        {
            BinaryReader reader = null;

            try
            {
                Stream streamIn = File.OpenRead(openFile);
                reader = new BinaryReader(streamIn);

                //Create a new map with the data
                CreateMap();

                //Read all the color data
                for (int i = 0; i < height; i++)
                {
                    for (int ii = 0; ii < width; ii++)
                    {
                        switch(reader.ReadInt32())
                        {
                            //Floor
                            case 0:
                                mapArray[i, ii].BackColor = Color.FromArgb(-6695580);
                                break;
                            //Wall
                            case 1:
                                mapArray[i, ii].BackColor = Color.FromArgb(-14513374);
                                break;
                            //Entrance
                            case 2:
                                mapArray[i, ii].BackColor = Color.FromArgb(-8388480);
                                break;
                            //Water
                            case 3:
                                mapArray[i, ii].BackColor = Color.FromArgb(-16776961);
                                break;
                            //Rock
                            case 4:
                                mapArray[i, ii].BackColor = Color.FromArgb(-4144960);
                                break;
                            //Stump
                            case 5:
                                mapArray[i, ii].BackColor = Color.FromArgb(-11123140);
                                break;
                            //Bridge
                            case 6:
                                mapArray[i, ii].BackColor = Color.FromArgb(-128);
                                break;
                            //Flower
                            case 7:
                                mapArray[i, ii].BackColor = Color.FromArgb(-65281);
                                break;
                            case 8:
                                mapArray[i, ii].BackColor = Color.FromArgb(-65536);
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                }

                int enemyCount = reader.ReadInt32();

                for(int i = 0; i < enemyCount; i++)
                {
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    int type = reader.ReadInt32();

                    switch(type)
                    {
                        case 0:
                            mapArray[y, x].Image = enemy0.Image;
                            break;
                        case 1:
                            mapArray[y, x].Image = enemy1.Image;
                            break;
                        case -1:
                            mapArray[y, x].Image = enemyMinus1.Image;
                            break;
                        case 2:
                            mapArray[y, x].Image = enemy2.Image;
                            break;
                        case -2:
                            mapArray[y, x].Image = enemyMinus2.Image;
                            break;
                    }
                }

                //Message shows up if it didn't fuck up the loading process
                MessageBox.Show("File Loaded Successfully", "File Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Mark the file as saved
                fileNameTitle = true;
                unsavedChanges = false;
                this.Text = string.Format("Level Editor - {0}", openFile);
                fileName = openFile;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Error writing to file: ", exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }



        /// <summary>
        /// Make sure we don't have a sad gamer moment and accidentally lose any progress by closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseEvent(object sender, FormClosingEventArgs e)
        {
            //Throw a fit if there are unsaved changes
            if(unsavedChanges)
            {
                DialogResult result = MessageBox.Show("You have unsaved changes! Are you sure you want to quit?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if(result != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Switch pallette pages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void palletteButton_Click(object sender, EventArgs e)
        {
            this.pallette++;
            if (pallette > palletteMax)
            {
                pallette = 0;
            }

            switch (pallette)
            {
                case 0:
                    this.colorBox.Visible = true;
                    this.enemyBox.Visible = false;
                    break;
                case 1:
                    this.colorBox.Visible = false;
                    this.enemyBox.Visible = true;
                    break;
            }

        }

        /// <summary>
        /// Clears the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButton_Click(object sender, EventArgs e)
        {
            //Throw a fit if there are unsaved changes
            if (unsavedChanges)
            {
                DialogResult panic = MessageBox.Show("You have unsaved changes! Are you sure you want to continue?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (panic != DialogResult.Yes)
                {
                    return;
                }
            }

            CreateMap();
            unsavedChanges = false;
            this.Text = "Level Editor";
        }
    }
}
