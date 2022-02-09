using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris_Toxiques
{
    public partial class Form1 : Form
    {
        Bitmap canvasBitmap;
        Graphics canvasGraphics;
        // Nouveaux Bitmap et Graphics pour pouvoir gérer le dessin de la forme.
        Bitmap shapeBitmap;
        Graphics shapeGraphics;
        // Autres Bitmap et Graphics pour gérer la pictureBox qui affiche la forme suivante.
        Bitmap nextShapeBitmap;
        Graphics nextShapeGraphics;
        // On crée la forme suivante.
        Shape nextShape;
        // On crée notre forme "actuelle" afin de pouvoir lui appliquer les méthodes que l'on a créées.
        Shape currentShape;
        // On crée une propriété de type Timer afin de pouvoir gérer la descente de la forme.
        Timer timer = new Timer();
        // On définie la hauteur et la largeur de notre air de jeu. D'après les normes, c'est du 10x22 mais sur de nombreux exemples, c'est plutôt du 10x20.
        int canvasWidth = 10;
        int canvasHeight = 20;
        // On crée un tableau dynamique à deux dimensions qui sera rempli dans notre méthode loadCanvas.
        int[,] canvasCaseArray;
        // On fixe la taille d'une case.
        int caseSize = 25;
        // On déclare deux variables x et y pour placer la forme dans notre aire de jeu.
        int posX;
        int posY;
        // Déclaration des variables qui gèreront le score et le niveau.
        int score;
        double lvl;

        //private static Brush[] colors = { Brushes.Lime, Brushes.Yellow, Brushes.Red, Brushes.DarkViolet, Brushes.Aqua, Brushes.DeepPink, Brushes.Snow };

        // Création du constructeur.
        public Form1()
        {
            InitializeComponent();

            // On crée et affiche le canvas.
            LoadCanvas();

            // On appelle la méthode getShapeRandomlyAndCenterIt pour donner une forme alétoire à notre forme actuelle.
            currentShape = GetShapeRandomlyAndCenterIt();
            nextShape = GetNextShape();

            // On ajoute l'événement qui s'effectuera à chaque "tick" (toutes les 500 millisecondes).
            timer.Tick += Timer_Tick;
            // On met un interval de 500 millisecondes à notre timer.
            timer.Interval = 500;
            timer.Start();

            this.KeyDown += Form1_KeyDown;
        }


        // On crée la méthode loadCanvas.
        private void LoadCanvas()
        {
            // On fixe la taille de notre pictureBox en multipliant le nombre de cases en largeur/hauteur par la taille d'une case.
            pictureBox1.Width = canvasWidth * caseSize;
            pictureBox1.Height = canvasHeight * caseSize;

            // On crée un Bitmap, pour manipuler une image en fonction de pixel, de la taille de notre pictureBox.
            canvasBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            // On crée un Graphic en fonction de l'image Bitmap.
            canvasGraphics = Graphics.FromImage(canvasBitmap);

            // On rempli le canvas Graphic en mettant une couleur bleue ciel au rectangle et en lui donnant une paire de coordonnées, de largeur et de hauteur.
            canvasGraphics.FillRectangle(Brushes.Gainsboro, 0, 0, canvasBitmap.Width, canvasBitmap.Height);

            // On fait en sorte que l'image de notre Bitmap se charge dans notre pictureBox.
            pictureBox1.Image = canvasBitmap;

            // On initialise le tableau de points en lui donnant une taille correspond à la hauteur et la largeur de notre air de jeu.
            // En gros, notre tableau 2D aura une largeur de 10 et une hauteur de 22.
            canvasCaseArray = new int[canvasWidth,canvasHeight];

        }

        // On crée une méthode qui va créer une forme aléatoire et la placer au milieu de l'aire de jeu.
        private Shape GetShapeRandomlyAndCenterIt()
        {
            // On crée une forme aléatoire.
            var shape = ShapesHandler.GetRandomShape();
            //shape.color = GetColorRandomly();

            // On place la forme au milieu de notre aire de jeu.
            posX = 4; // On la met à la moitié de la largeur de notre aire de jeu.
            posY = -shape.height; // On la fait démarrer au-dessus de l'aire de jeu pour avoir la sensation qu'elle apparaît d'en haut.

            return shape;
        }

        // On crée une méthode qui nous permettra de déplacer la forme d'une case vers le bas et de gérer les collisions.
        private bool MoveShape(int moveDown = 0, int moveSide = 0)
        {
            // On donne de nouvelles coordonnées à notre forme.
            var newPosX = posX + moveSide;
            var newPosY = posY + moveDown;

            // On vérifie si, après le mouvement, la forme sort de l'aire de jeu ou si elle atteint le bas de l'aire de jeu.
            if (newPosX < 0 || newPosX + currentShape.width > canvasWidth || newPosY + currentShape.height > canvasHeight)
            {
                return false;
            }

            // On vérifie si notre forme actuelle touche une autre forme.
            for (int i = 0; i < currentShape.width; i++)
            {
                for (int j = 0; j < currentShape.height; j++)
                {
                    // Ici on vérifie que notre forme ne touche pas le bas de l'aire de jeu, que la case où doit se déplacer notre forme n'est pas remplie.
                    if (newPosY + j > 0 && canvasCaseArray[newPosX + i,newPosY + j] == 1 && currentShape.cases[j,i] == 1)
                    {
                        return false;
                    }
                }
            }

            posX = newPosX;
            posY = newPosY;

            DrawShape();

            return true;
        }

        // On crée une méthode qui nous permet de redessiner la forme.
        private void DrawShape()
        {
            // On crée notre Bitmap pour la forme en partant du Bitmap de l'aire de jeu.
            shapeBitmap = new Bitmap(canvasBitmap);
            shapeGraphics = Graphics.FromImage(shapeBitmap);

            for (int i = 0; i < currentShape.width; i++)
            {
                for (int j = 0; j < currentShape.height; j++)
                {
                    // On colorie les cases correspondantes à notre forme en fonction de si c'est un 1 ou en 0 (1 = colorié, 0 = blanc).
                    if (currentShape.cases[j,i] == 1)
                    {
                        // On remplie une des cases qui correspond à un '1' de notre forme en fonction de sa position actuelle.
                        shapeGraphics.FillRectangle(currentShape.color, (posX + i) * caseSize, (posY + j) * caseSize, caseSize, caseSize);
                    }
                }
            }

            pictureBox1.Image = shapeBitmap;
        }

        // Cette méthode permet de mettre à jour la zone de dessin (canvas) en mettant à '1' les cases remplies par la forme actuelle.
        private void UpdateCanvasCasesArrayWithCurrentShape()
        {
            for (int i = 0; i < currentShape.width; i++)
            {
                for (int j = 0; j < currentShape.height; j++)
                {
                    if (currentShape.cases[j,i] == 1)
                    {
                        CheckIfGameOver();
                        canvasCaseArray[posX + i,posY + j] = 1;
                    }
                }
            }
        }

        // Méthode pour vérifier si la partie est perdue (Game Over).
        private void CheckIfGameOver()
        {
            if (posY < 0)
            {
                timer.Stop();
                MessageBox.Show("Game Over");
                Application.Exit();
            }
        }

        // Méthode qui permet à la forme de se déplacer en fonction du temps.
        private void Timer_Tick(object envoyeur, EventArgs e) // EventArgs est une classe qui permet de gérer les événements.
        {
            var isMoveSuccess = MoveShape(moveDown: 1); // Au lieu d'initialiser moveDown à 0 automatiquement, on le met à 1.

            // On vérifie que notre forme ne peut plus bouger.
            if (!isMoveSuccess)
            {
                // On copie l'image de la forme actuelle dans notre aire de jeu.
                canvasBitmap = new Bitmap(shapeBitmap);
                // On met à jour l'aire de jeu.
                UpdateCanvasCasesArrayWithCurrentShape();
                // On récupère la forme suivante.
                currentShape = nextShape;
                nextShape = GetNextShape();

                ClearFilledRowsAndUpdateScore();
            }
        }

        // Méthode qui gère les événements claviers (interaction avec les flèches).
        private void Form1_KeyDown (object envoyeur, KeyEventArgs e)
        {
            // On déclare deux variables afin de gérer le déplacement vertical et horizontal de la forme.
            var moveSide = 0;
            var moveDown = 0;

            // On effectue un switch sur les touches préssées afin de gérer les événements.
            switch (e.KeyCode)
            {
                // Déplacement vers la gauche.
                case Keys.Left:
                    moveSide--; 
                    break;
             
                // Déplacement vers la droite.
                case Keys.Right:
                    moveSide++; 
                    break;

                // Déplacement vers le bas.
                case Keys.Down:
                    moveDown++; 
                    break;

                // Rotation de la forme.
                case Keys.Up: 
                    currentShape.turn(); 
                    break;

                default: 
                    return;
            }

            // On déplace la forme.
            var isMoveSuccess = MoveShape(moveDown, moveSide);

            // Si le joueur essaye de tourner la forme mais que c'est impossible, on revient en arrière.
            if (!isMoveSuccess && e.KeyCode == Keys.Up)
            {
                currentShape.rollback();
            }
        }

        // Méthode qui permet de supprimer une ligne remplie et d'incrémenter le score.
        public void ClearFilledRowsAndUpdateScore()
        {
            // Gestion du niveau.
            lvl = score / 100;
            // On parcourt l'aire de jeu afin de voir s'il y a au moins une ligne remplie.
            for (int i = 0; i < canvasHeight; i++)
            {
                int j;
                for (j = canvasWidth - 1; j >= 0; j--)
                {
                    // Si la ligne n'est pas remplie, on passe à la ligne suivante.
                    if (canvasCaseArray[j, i] == 0)
                        break;
                }

                // Si la ligne est remplie.
                if (j == -1)
                {
                    // On augmente le score, on met à jour les labels et on accélère le jeu.
                    score += 50;
                    label2.Text = "Score : " + score;
                    label3.Text = "Level : " + score/100;

                    // Si le niveau a augmenté (tous les 100 pts), on accélère.
                    if (lvl != score/100)
                    {
                        timer.Interval -= 20;
                    }

                    // On met à jour l'aire de jeu.
                    for (j = 0; j < canvasWidth; j++)
                    {
                        for (int k = i; k > 0; k--)
                        {
                            // La ligne actuelle devient la ligne d'au-dessus.
                            canvasCaseArray[j, k] = canvasCaseArray[j, k - 1];
                        }

                        // La première ligne est remise à 0 de partout.
                        canvasCaseArray[j, 0] = 0;
                    }
                }
            }

            // On redessine l'aire de jeu en fonction des modifications apportées juste au-dessus.
            for (int i = 0; i < canvasWidth; i++)
            {
                for (int j = 0; j < canvasHeight; j++)
                {
                    canvasGraphics = Graphics.FromImage(canvasBitmap);
                    // Là on dit, si la case est un 1, on doit la colorier, sinon on laisse la couleur de fond. C'est un opérateur ternaire.
                    if (canvasCaseArray[i, j] == 0)
                    {
                        canvasGraphics.FillRectangle(Brushes.Gainsboro, i * caseSize, j * caseSize, caseSize, caseSize);
                    }
                }
            }

            pictureBox1.Image = canvasBitmap;
        }

        private Shape GetNextShape()
        {
            var shape = GetShapeRandomlyAndCenterIt();
            //shape.color = GetColorRandomly();
            

            // On affiche la forme à venir dans la pictureBox à droite du jeu.
            nextShapeBitmap = new Bitmap(7 * caseSize, 6 * caseSize);
            nextShapeGraphics = Graphics.FromImage(nextShapeBitmap);
            nextShapeGraphics.FillRectangle(Brushes.Gainsboro, 0, 0, nextShapeBitmap.Width, nextShapeBitmap.Height);

            // On centre la forme dans la pictureBox.
            var startX = (8 - shape.width) / 2;
            var startY = (6 - shape.height) / 2;

            for (int i = 0; i < shape.height; i++)
            {
                for (int j = 0; j < shape.width; j++)
                {
                    // Comme plus haut, opérateur ternaire pour dire que s'il y a un 1, on colorie, sinon on laisse la couleur de fond.
                    nextShapeGraphics.FillRectangle(shape.cases[i, j] == 1 ? shape.color : Brushes.Gainsboro, (startX + j) * caseSize, (startY + i) * caseSize, caseSize, caseSize);
                }
            }

            pictureBox2.Size = nextShapeBitmap.Size;
            pictureBox2.Image = nextShapeBitmap;

            return shape;
        }

        /*// Cette méthode permet de donner à notre forme une couleur aléatoire.
        public static Brush GetColorRandomly()
        {
            Random rnd = new Random();
            int rand = rnd.Next(0, colors.Length);
            return colors[rand];
        }*/
    }
}
