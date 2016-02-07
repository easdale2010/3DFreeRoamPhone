using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch; // Include for Windows Phone games
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace gamelib2d
{
    // Class for 2D graphics
    public class graphic2d
    {
        private Texture2D image;                 // Texture to hold image
        public Rectangle rect;                  // Rectangle to hold position & size of the image

        public graphic2d() { }  // Empty constructor to avoid crashes

        // Constructor which loads image and fits the background to fill the width of the screen
        public graphic2d(ContentManager content, string spritename, int dwidth, int dheight)
        {
            image = content.Load<Texture2D>(spritename);
            float ratio = ((float)dwidth / image.Width);
            rect.Width = dwidth;
            rect.Height = (int)(image.Height * ratio);
            rect.X = 0;
            rect.Y = (dheight - rect.Height) / 2;
        }
        
        // Load a jpeg or png from a file not in the content pipeline
        public graphic2d(GraphicsDeviceManager graphics, ContentManager content, string spritename, int dwidth, int dheight)
        {
            Stream picstream = File.Open(spritename, FileMode.Open);
            image = Texture2D.FromStream(graphics.GraphicsDevice, picstream); // Load an image from the HDD not in the content pipeline
            picstream.Close();
            float ratio = ((float)dwidth / image.Width);    // Work out the ratio for the image depending on screen size
            rect.Width = dwidth;                            // Set image width to match the screen width
            rect.Height = (int)(image.Height * ratio);      // Work out new height based on the screen aspect ratio
            rect.X = 0;
            rect.Y = (dheight - rect.Height) / 2;           // Put image in the middle of the screen on the Y axis
        }

        // Stretch the image to fit the screen exactly
        public void stretch2fit(int dwidth, int dheight)
        {
            rect.Width = dwidth;
            rect.Height = dheight;
            rect.X = 0;
            rect.Y = 0;
        }

        // Use this method to draw the image
        public void drawme(ref SpriteBatch spriteBatch2)
        {
            spriteBatch2.Draw(image, rect, Color.White);
        }
    }


    // Class for 2D sprites
    public class sprite2d
    {
        public Texture2D image;         		// Texture which holds image
        public Vector3 position; 		 	    // Position on screen
        public Vector3 oldposition;             // Old position before collisions
        public Rectangle rect;          		// Rectangle to hold size and position
        public Vector2 origin;          		// Centre point
        public float rotation = 0;          	// Amount of rotation to apply
        public float rotspeed = 0.05f;          // Speed they should spin at
        public Vector3 velocity;        		// Velocity (Direction and speed)
        public BoundingSphere bsphere;  		// Bounding sphere
        public BoundingBox bbox;                // Bounding box
        public Boolean visible = true;    		// Should object be drawn true or false
        public Color colour = Color.White;      // Holds colour to draw the image in
        public float size;                      // Size ratio of object

        public sprite2d() { }                   // Empty constructor to avoid crashes

        // Constructor which initialises the sprite2D
        public sprite2d(ContentManager content, string spritename, int x, int y, float msize, Color mcolour, Boolean mvis)
        {
            image = content.Load<Texture2D>(spritename);    // Load image into texture
            position = new Vector3((float)x, (float)y, 0);  // Set position
            rect.X = x;                                     // Set position of draw rectangle x
            rect.Y = y;                                     // Set position of draw rectangle y
            origin.X = image.Width / 2;               	    // Set X origin to half of width
            origin.Y = image.Height / 2;              	    // Set Y origin to half of height
            rect.Width = (int)(image.Width * msize);  	    // Set the new width based on the size ratio 
            rect.Height = (int)(image.Height * msize);	    // Set the new height based on the size ratio
            colour = mcolour;                               // Set colour
            visible = mvis;                                 // Image visible TRUE of FALSE? 
            size = msize;                                   // Store size ratio
            oldposition = position;
            updateobject();
        }

        public void automove(int dwidth, int dheight, float gtime)
        {
            // Add code here for when the game is running
            rotation += rotspeed; // Spin Ball
            position += velocity * gtime; // Add current velocity to the position of the ball

            // Check if the ball hits any of the four sides and bounce it off them
            if ((position.X + rect.Width / 2) >= dwidth)
            {
                velocity.X = -velocity.X;
                position.X = dwidth - rect.Width / 2;
            }
            if ((position.X - rect.Width / 2) <= 0)
            {
                velocity.X = -velocity.X;
                position.X = rect.Width / 2;
            }
            if ((position.Y + rect.Height / 2) >= dheight)
            {
                velocity.Y = -velocity.Y;
                position.Y = dheight - rect.Height / 2;
            }
            if ((position.Y - rect.Height / 2) <= 0)
            {
                velocity.Y = -velocity.Y;
                position.Y = rect.Height / 2;
            }
            updateobject();
        }

        public void moveme(GamePadState gpad, float gtime, int dwidth, int dheight)
        {
            if (visible)
            {
                // Basic Movement Left, Right, Up, Down
                velocity.X = gpad.ThumbSticks.Left.X;
                velocity.Y = -gpad.ThumbSticks.Left.Y;

                float speed = 0.5f;
                position += velocity * gtime * speed;   // Set position based on velocity, time between updates and speed

                // Set screen limits for object
                if (position.X < rect.Width / 2) position.X = rect.Width / 2;
                if (position.X > dwidth - rect.Width / 2) position.X = dwidth - rect.Width / 2;
                if (position.Y < rect.Height / 2) position.Y = rect.Height / 2;
                if (position.Y > dheight - rect.Height / 2) position.Y = dheight - rect.Height / 2;

                updateobject();
            }
        }

        public void updateobject()
        {
            // Set position of object into the rectangle from the position Vector2
            rect.X = (int)position.X;
            rect.Y = (int)position.Y;
            // Create Boundingsphere around the object
            bsphere = new BoundingSphere(position, rect.Width / 2);
            // Create Boundingbox around the object
            bbox = new BoundingBox(new Vector3(position.X - rect.Width / 2, position.Y - rect.Height / 2, 0),
                                    new Vector3(position.X + rect.Width / 2, position.Y + rect.Height / 2, 0));
        }

        // Use this method to draw the image
        public void drawme(ref SpriteBatch sbatch)
        {
            if (visible)
                sbatch.Draw(image, rect, null, colour, rotation, origin, SpriteEffects.None, 0);
        }

        // Use this method to draw the image at a specified position
        public void drawme(ref SpriteBatch sbatch, Vector3 newpos)
        {
            if (visible)
            {
                Rectangle newrect = rect;
                newrect.X = (int)newpos.X;
                newrect.Y = (int)newpos.Y;

                sbatch.Draw(image, newrect, null, colour, rotation, origin, SpriteEffects.None, 0);
            }
        }
    }


    // Class for spaceships or any other overhead 2D vehicle
    public class ships : sprite2d
    {
        public Vector3 direction;       // Direction ship is facing
        private float thrust;                   // Amount of thrust applied
        private float rotationspeed = 0.005f;   // Rotation speed of ship
        private float shipspeed = 0.01f;        // Ship speed
        private float friction = 0.99f;         // Amount of friction to apply
        public int lives = 5;           // Amount of lives each ship has
        public int score = 0;           // Score for each ship
        public float spawntime = 0;     // Time since spawned last

        public ships() { }              // Empty constructor to avoid crashes

        // Constructor which fits image to screen resolution and keeps aspect ratio the same
        public ships(ContentManager content, string spritename, int x, int y, float msize, Color mcolour, Boolean mvis)
        {
            image = content.Load<Texture2D>(spritename);    // Load image into texture
            position = new Vector3((float)x, (float)y, 0);  // Set position
            rect.X = x;                                     // Set position of draw rectangle x
            rect.Y = y;                                     // Set position of draw rectangle y
            origin.X = image.Width / 2;               	    // Set X origin to half of width
            origin.Y = image.Height / 2;              	    // Set Y origin to half of height
            rect.Width = (int)(image.Width * msize);  	    // Set the new width based on the size ratio 
            rect.Height = (int)(image.Height * msize);	    // Set the new height based on the size ratio
            colour = mcolour;                               // Set colour
            visible = mvis;                                 // Image visible TRUE of FALSE? 
            size = msize;                                   // Store size ratio
            oldposition = position;
        }

        public void moveme(GamePadState gpad, int dwidth, int dheight, float gtime)
        {
            spawntime += gtime;     // Count time since last spawned

            rotation += gpad.ThumbSticks.Left.X * rotationspeed * gtime;    // Spin the ship based on the left stick
            thrust = (shipspeed * gtime * (gpad.Triggers.Right - gpad.Triggers.Left)); // Work out how much forward or backward thrust to apply

            // Work out the direction the ship is facing
            direction.X = (float)(Math.Cos(rotation));
            direction.Y = (float)(Math.Sin(rotation));

            velocity += direction * thrust;     // Add direction*thrust to velocity
            velocity *= friction;               // Reduce velocity by applying friction
            position += velocity;               // Add current velocity to the position of the ball

            // Set screen limits for object
            if (position.X < rect.Width / 2) position.X = rect.Width / 2;
            if (position.X > dwidth - rect.Width / 2) position.X = dwidth - rect.Width / 2;
            if (position.Y < rect.Height / 2) position.Y = rect.Height / 2;
            if (position.Y > dheight - rect.Height / 2) position.Y = dheight - rect.Height / 2;

            updateobject();
        }

        public void movemess(GamePadState gpad, int dwidth, int dheight, float gtime)
        {
            spawntime += gtime;     // Count time since last spawned

            // Work out the direction the user wants the ship to move in
            direction.X = gpad.ThumbSticks.Left.X;
            direction.Y = -gpad.ThumbSticks.Left.Y;

            velocity += direction * shipspeed * gtime;     // Add direction*thrust to velocity
            velocity *= friction;               // Reduce velocity by applying friction
            position += velocity;               // Add current velocity to the position of the ball

            // Set screen limits for object
            if (position.X < rect.Width / 2) position.X = rect.Width / 2;
            if (position.X > dwidth - rect.Width / 2) position.X = dwidth - rect.Width / 2;
            if (position.Y < rect.Height / 2) position.Y = rect.Height / 2;
            if (position.Y > dheight - rect.Height / 2) position.Y = dheight - rect.Height / 2;

            updateobject();
        }

    }

    // Class for bullets and the like
    public class ammo : sprite2d
    {
        private float bulletlength = 1000;              // Time that the bullet fires for
        private float bulletspawned = 1001;             // Time since the bullet was last spawned
        private float bulletspeed = 1.5f;               // Bullet speed

        public ammo() { }                       // Empty constructor to avoid crashes

        // Constructor which initialises the sprite2D
        public ammo(ContentManager content, string spritename, int x, int y, float msize, Color mcolour, Boolean mvis, float bullspeed)
        {
            image = content.Load<Texture2D>(spritename);    // Load image into texture
            position = new Vector3((float)x, (float)y, 0);  // Set position
            rect.X = x;                                     // Set position of draw rectangle x
            rect.Y = y;                                     // Set position of draw rectangle y
            origin.X = image.Width / 2;               	    // Set X origin to half of width
            origin.Y = image.Height / 2;              	    // Set Y origin to half of height
            rect.Width = (int)(image.Width * msize);  	    // Set the new width based on the size ratio 
            rect.Height = (int)(image.Height * msize);	    // Set the new height based on the size ratio
            colour = mcolour;                               // Set colour
            visible = mvis;                                 // Image visible TRUE of FALSE? 
            size = msize;                                   // Store size ratio
            oldposition = position;
            bulletspeed = bullspeed;                        // Set the speed of the bullets
        }

        public void firebullet(Vector3 pos, Vector3 dir)
        {
            if (!visible)
            {
                visible = true;
                position = pos;
                velocity = dir * bulletspeed;
                updateobject(); // I forgot to add this in the notes, which occasionaly causes bullets to appear and kill things before they are moved
                bulletspawned = 0;
            }
        }

        public void movebullet(float gtime)
        {
            bulletspawned += gtime;     // Count the time since the bullet was spawned
            if (visible)
            {
                position += velocity * gtime;   // Add current velocity to the bullet

                // After 1 second bullet is reset for re-firing
                if (bulletspawned > bulletlength) visible = false;
                updateobject();
            }
        }
    }


    public class scrollingbackground
    {
        public Texture2D imagemain;                 // Texture to hold image
        public float scale;
        public int gamewidth;
        public int gameheight;
        public int columns = 1;
        public int rows = 1;

        public scrollingbackground() { }  // Empty constructor to avoid crashes

        // Constructor which loads image and works out the new gamewidth & gameheight. Also set the size of the main background rectangle and the size of the part2draw rectangle
        public scrollingbackground(ContentManager content, string spritename, float sizeratio, int colnum, int rownum)
        {
            imagemain = content.Load<Texture2D>(spritename);
            scale = sizeratio;
            columns = colnum;
            rows = rownum;
            gamewidth = (int)(imagemain.Width * columns * scale);
            gameheight = (int)(imagemain.Height * rows * scale);
        }

        public void makehorizontal(int dheight)
        {
            rows = 1;
            gameheight = dheight;
            scale = (float)gameheight / (float)imagemain.Height;
            gamewidth = (int)(imagemain.Width * columns * scale);
        }

        public void makevertical(int dwidth)
        {
            columns = 1;
            gamewidth = dwidth;
            scale = (float)gamewidth / (float)imagemain.Width;
            gameheight = (int)(imagemain.Height * rows * scale);
        }

        // Use this method to draw a part of the image at a specific position
        virtual public Vector3 drawme(ref SpriteBatch sbatch, Vector3 position, int dwidth, int dheight, out Vector3 offset)
        {
            Vector3 corner = new Vector3(0, 0, 0);
            Rectangle panel = new Rectangle(0, 0, 0, 0);
            panel.Width = (int)(imagemain.Width * scale);
            panel.Height = (int)(imagemain.Height * scale);
            Vector3 screenpos = new Vector3((float)(dwidth / 2), (float)(dheight / 2), 0);

            // Check if ship is near the edges of the game world and adjust it's position
            if (position.X < dwidth / 2)
                screenpos.X = position.X;
            if (position.Y < dheight / 2)
                screenpos.Y = position.Y;
            if (position.X > gamewidth - dwidth / 2)
                screenpos.X = dwidth - (gamewidth - position.X);
            if (position.Y > gameheight - dheight / 2)
                screenpos.Y = dheight - (gameheight - position.Y);

            // Loop through all the background panels in the game and draw them
            for (int x = 0; x < columns; x++)
            {
                corner.X = position.X - screenpos.X;
                panel.X = (int)(x * imagemain.Width * scale - corner.X);
                for (int y = 0; y < rows; y++)
                {
                    corner.Y = position.Y - screenpos.Y;
                    panel.Y = (int)(y * imagemain.Height * scale - corner.Y);

                    // If panel is within the visible screen draw it
                    if ((panel.X < gamewidth || panel.Y < gameheight) && (panel.Right > 0 || panel.Bottom > 0))
                        sbatch.Draw(imagemain, panel, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
                }
            }

            // Work out the position offset amount for everything else in the game
            offset = new Vector3(position.X - screenpos.X, position.Y - screenpos.Y, 0);

            return screenpos;
        }
    }

    public class scrollingbackground2 : scrollingbackground
    {
        public Texture2D[,] image;                 // Texture to hold image

        public scrollingbackground2() { }  // Empty constructor to avoid crashes

        // Constructor which loads image and works out the new gamewidth & gameheight. Also set the size of the main background rectangle and the size of the part2draw rectangle
        public scrollingbackground2(ContentManager content, string spritename, float sizeratio, int colnum, int rownum)
        {
            columns = colnum;
            rows = rownum;
            image = new Texture2D[colnum, rownum];
            imagemain = content.Load<Texture2D>(spritename);
            for (int x = 0; x < colnum; x++)
                for (int y = 0; y < rownum; y++)
                {
                    image[x, y] = content.Load<Texture2D>(spritename);
                }
            scale = sizeratio;
            gamewidth = (int)(image[0, 0].Width * columns * scale);
            gameheight = (int)(image[0, 0].Height * rows * scale);
        }

        // Use this method to draw a part of the image at a specific position
        override public Vector3 drawme(ref SpriteBatch sbatch, Vector3 position, int dwidth, int dheight, out Vector3 offset)
        {
            Vector3 corner = new Vector3(0, 0, 0);
            Rectangle panel = new Rectangle(0, 0, 0, 0);
            panel.Width = (int)(image[0, 0].Width * scale);
            panel.Height = (int)(image[0, 0].Height * scale);
            Vector3 screenpos = new Vector3((float)(dwidth / 2), (float)(dheight / 2), 0);

            // Check if ship is near the edges of the game world and adjust it's position
            if (position.X < dwidth / 2)
                screenpos.X = position.X;
            if (position.Y < dheight / 2)
                screenpos.Y = position.Y;
            if (position.X > gamewidth - dwidth / 2)
                screenpos.X = dwidth - (gamewidth - position.X);
            if (position.Y > gameheight - dheight / 2)
                screenpos.Y = dheight - (gameheight - position.Y);

            // Loop through all the background panels in the game and draw them
            for (int x = 0; x < columns; x++)
            {
                corner.X = position.X - screenpos.X;
                panel.X = (int)(x * image[0, 0].Width * scale - corner.X);
                for (int y = 0; y < rows; y++)
                {
                    corner.Y = position.Y - screenpos.Y;
                    panel.Y = (int)(y * image[0, 0].Height * scale - corner.Y);

                    // If panel is within the visible screen draw it
                    if ((panel.X < gamewidth || panel.Y < gameheight) && (panel.Right > 0 || panel.Bottom > 0))
                        sbatch.Draw(image[x, y], panel, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
                }
            }

            // Work out the position offset amount for everything else in the game
            offset = new Vector3(position.X - screenpos.X, position.Y - screenpos.Y, 0);

            return screenpos;
        }
    }


    // Class for 2D animation
    public class animation
    {
        private Texture2D image;            // Texture which holds animation sheet
        public Vector3 position;    // Position of animation
        public Rectangle rect;      // Rectangle to hold size and position
        private Rectangle frame_rect;       // Rectangle to hold position of frame to draw
        private Vector2 origin;             // Centre point
        public float rotation = 0;  // Rotation amount
        public Color colour = Color.White; // Colour
        public float size;          // Size Ratio
        public Boolean visible;     // Should object be drawn true or false
        public int framespersecond; // Frame Rate
        private int frames;                 // Number of frames of animation
        private int rows;                   // Number of rows in the sprite sheet
        private int columns;                // Number of columns in the sprite sheet
        private int frameposition;          // Current position in the animation
        private int framewidth;             // Width in pixels of each frame of animation
        private int frameheight;            // Height in pixels of each frame of animation
        private float timegone;             // Time since animation began
        private Boolean loop = false;// Should animation loop
        private int noofloops = 0;          // Number of loops to do
        private int loopsdone = 0;          // Number of loops completed
        public Boolean paused = false;  // Freeze frame animation
        private Boolean playbackwards = false;  // Sets whether animation should play forwards or backwards
        private Boolean fliphorizontal = false; // Should image be flipped horizontally

        public animation() { }


        // Constructor which initialises the animation
        public animation(ContentManager content, string spritename, int x, int y, float msize, Color mcolour, Boolean mvis, int fps, int nrows, int ncol)
        {
            image = content.Load<Texture2D>(spritename);    // Load image into texture
            position = new Vector3((float)x, (float)y, 0);  // Set position
            rect.X = x;                                     // Set position of draw rectangle x
            rect.Y = y;                                     // Set position of draw rectangle y
            size = msize;                                   // Store size ratio
            colour = mcolour;                               // Set colour
            visible = mvis;                                 // Image visible TRUE of FALSE? 
            framespersecond = fps;                          // Store frames per second
            rows = nrows;                                   // Number of rows in the sprite sheet
            columns = ncol;                                 // Number of columns in the sprite sheet
            frames = rows * columns;                          // Store no of frames
            framewidth = (int)(image.Width / columns);      // Calculate the width of each frame of animation
            frameheight = (int)(image.Height / rows);       // Calculate the heigh of each frame of animation
            rect.Width = (int)(framewidth * size);          // Set the new width based on the size ratio    
            rect.Height = (int)(frameheight * size);	    // Set the new height based on the size ratio
            frame_rect.Width = framewidth;                  // Set the width of each frame
            frame_rect.Height = frameheight;                // Set the height of each frame
            origin.X = framewidth / 2;                      // Set X origin to half of frame width
            origin.Y = frameheight / 2;              	    // Set Y origin to half of frame heigh
        }


        // Constructor which initialises the animation
        public animation(ContentManager content, string spritename, int x, int y, float msize, Color mcolour, Boolean mvis, int fps, int nrows, int ncol, Boolean loopit, Boolean playback, Boolean drawreversed)
        {
            image = content.Load<Texture2D>(spritename);    // Load image into texture
            position = new Vector3((float)x, (float)y, 0);  // Set position
            rect.X = x;                                     // Set position of draw rectangle x
            rect.Y = y;                                     // Set position of draw rectangle y
            size = msize;                                   // Store size ratio
            colour = mcolour;                               // Set colour
            visible = mvis;                                 // Image visible TRUE of FALSE? 
            framespersecond = fps;                          // Store frames per second
            rows = nrows;                                   // Number of rows in the sprite sheet
            columns = ncol;                                 // Number of columns in the sprite sheet
            frames = rows * columns;                          // Store no of frames
            framewidth = (int)(image.Width / columns);      // Calculate the width of each frame of animation
            frameheight = (int)(image.Height / rows);       // Calculate the heigh of each frame of animation
            rect.Width = (int)(framewidth * size);          // Set the new width based on the size ratio    
            rect.Height = (int)(frameheight * size);	    // Set the new height based on the size ratio
            frame_rect.Width = framewidth;                  // Set the width of each frame
            frame_rect.Height = frameheight;                // Set the height of each frame
            origin.X = framewidth / 2;                      // Set X origin to half of frame width
            origin.Y = frameheight / 2;              	    // Set Y origin to half of frame heigh
            loop = loopit;                                  // Should it be looped or not
            playbackwards = playback;                       // Should animation be played forwards or backwards
            fliphorizontal = drawreversed;                  // Should the animation be drawn flipped horizontally
        }

        public void start(Vector3 pos)
        {
            // Set position of object into the rectangle from the position Vector
            position = pos;
            rect.X = (int)position.X;
            rect.Y = (int)position.Y;

            // Start new animation
            noofloops = 0;
            visible = true;
            frameposition = 0;
            timegone = 0;
            loopsdone = 0;
            paused = false;
        }

        public void start(Vector3 pos, float rot, int repeatnumber)
        {
            // Set position of object into the rectangle from the position Vector
            position = pos;
            rect.X = (int)position.X;
            rect.Y = (int)position.Y;

            // Start new animation
            noofloops = repeatnumber;
            rotation = rot;
            visible = true;
            frameposition = 0;
            timegone = 0;
            loopsdone = 0;
            paused = false;
        }

        public void update(float gtime)
        {
            if (visible && !paused)
            {
                if (framespersecond > 0) // Error checking to avoid divide by zero
                {
                    int framepos = (int)(timegone / (1000 / framespersecond));  // Work out what frame the animation is on
                    if (framepos > frameposition) frameposition = framepos;     // Set new frame position only if it has advanced forward
                }

                // Inverse frame position number if you want to play the animation backwards
                if (playbackwards)
                    frameposition = (frames - 1) - frameposition;

                timegone += gtime;                                          // Time gone during the animation

                // Check if the animation is at the end
                if ((!playbackwards && frameposition >= frames) || (playbackwards && frameposition < 0))
                {
                    // Repeat animation if necessary
                    if (loop || loopsdone < noofloops)
                    {
                        loopsdone++;
                        if (!playbackwards)
                            frameposition = 0;
                        else
                            frameposition = frames - 1;
                        timegone = 0;
                    }
                    else
                    {
                        visible = false;   // End animation
                    }
                }
            }
        }

        // Use this method to draw the image
        public void drawme(ref SpriteBatch sbatch)
        {
            if (visible)
            {   // Work out the co-ordinates of the current frame and then draw that frame
                frame_rect.Y = ((int)(frameposition / columns)) * frameheight;
                frame_rect.X = (frameposition - ((int)(frameposition / columns)) * columns) * framewidth;

                if (fliphorizontal)
                    sbatch.Draw(image, rect, frame_rect, colour, rotation, origin, SpriteEffects.FlipHorizontally, 0);
                else
                    sbatch.Draw(image, rect, frame_rect, colour, rotation, origin, SpriteEffects.None, 0);
            }
        }

        // Use this method to draw the image at a specified position
        public void drawme(ref SpriteBatch sbatch, Vector3 newpos)
        {
            if (visible)
            {
                Rectangle newrect = rect;
                newrect.X = (int)newpos.X;
                newrect.Y = (int)newpos.Y;

                frame_rect.Y = ((int)(frameposition / columns)) * frameheight;
                frame_rect.X = (frameposition - ((int)(frameposition / columns)) * columns) * framewidth;
                if (fliphorizontal)
                    sbatch.Draw(image, newrect, frame_rect, colour, rotation, origin, SpriteEffects.FlipHorizontally, 0);
                else
                    sbatch.Draw(image, newrect, frame_rect, colour, rotation, origin, SpriteEffects.None, 0);
            }
        }

        // Use this method to draw the image at a specified position and allow image to be flipped horizontally or vertically
        public void drawme(ref SpriteBatch sbatch, Vector3 newpos, Boolean flipx, Boolean flipy)
        {
            if (visible)
            {
                Rectangle newrect = rect;
                newrect.X = (int)newpos.X;
                newrect.Y = (int)newpos.Y;

                frame_rect.Y = ((int)(frameposition / columns)) * frameheight;
                frame_rect.X = (frameposition - ((int)(frameposition / columns)) * columns) * framewidth;
                if (flipx)
                    sbatch.Draw(image, newrect, frame_rect, colour, rotation, origin, SpriteEffects.FlipHorizontally, 0);
                else if (flipy)
                    sbatch.Draw(image, newrect, frame_rect, colour, rotation, origin, SpriteEffects.FlipVertically, 0);
                else
                    sbatch.Draw(image, newrect, frame_rect, colour, rotation, origin, SpriteEffects.None, 0);
            }
        }
    }


    // Class for animated 2D moving sprite
    public class animatedsprite
    {
        public animation[] spriteanimation;     // Holds the animations for the sprite
        public Vector3 position; 		 	    // World position
        public Vector3 oldposition;             // Old position before collisions
        public Vector3 screenposition;          // Position on the screen
        public Vector3 velocity;        		// Velocity (Direction and speed)
        public BoundingBox bbox;  		        // Bounding Box
        public BoundingBox bboxold;             // Old Bounding Box
        public Boolean visible = true;    		// Should object be drawn true or false
        public int state = 0;                   // State that guy is in
        private float friction = 0.95f;                 // Amount of friction to apply when he is on the ground or platforms
        private float airresistance = 0.999f;           // Air resistance (friction while in flight)
        private float gravity = 2f;                     // Force of gravity to apply
        private float power = 1f;                       // Potential of character for speed
        private Boolean inair = false;                  // Sprite is in flight
        private int averageupdatetime = 33;             // Used in conjunction with gametime, set it to the average number of milliseconds on the platform you intend to deploy on
        private int numberofstates = 1;                 // Number of different states (animations) that the guy can be in
        private int oldstate = -1;                      // Previous state
        private int previousstate = -1;                 // Old state
        public float rotation = 0;              // Rotation amount

        public animatedsprite() { }             // Empty constructor to avoid crashes

        // Constructor to set initial position and old position
        public animatedsprite(Vector3 pos, float frictiontemp, float gravitytemp, float characterspeed, int numberofanimations)
        {
            numberofstates = numberofanimations;
            spriteanimation = new animation[numberofstates];
            position = pos;             // Set current position of character
            oldposition = pos;          // Set old position to current position
            friction = frictiontemp;    // Friction to apply in game
            gravity = gravitytemp;      // Set gravity
            power = characterspeed;     // Potential speed of character
        }

        // Moves the sprite left or right based on gamepad input
        public void move(GamePadState pad, float gtime, int gwidth, int gheight, Boolean allowmoveinair)
        {
            inair = !(velocity.Y >= 0 && velocity.Y <= 3);
            if (!inair || allowmoveinair)
            {
                velocity.X += pad.ThumbSticks.Left.X * power;   // Set velocity based on direction stick is pressed in
                KeyboardState keys = Keyboard.GetState();       // Read keyboard
                if (keys.IsKeyDown(Keys.Left)) velocity.X -= power;
                if (keys.IsKeyDown(Keys.Right)) velocity.X += power;

                // Set the state to indicate which way the sprite is facing, LEFT or RIGHT
                if (pad.ThumbSticks.Left.X < -0.1 || keys.IsKeyDown(Keys.Left))
                    state = 1;
                else if (pad.ThumbSticks.Left.X > 0.1 || keys.IsKeyDown(Keys.Right))
                    state = 0;
            }
            updatesprite(gtime, gwidth, gheight); // Update the sprites position and apply physics
        }


        // Include this for Windows Phone games
        // Moves the sprite left or right based touch pad input
        public void move(float gtime, int gwidth, int gheight, Boolean allowmoveinair)
        {
            float directionX = 0;

            TouchCollection tcoll = TouchPanel.GetState();
            foreach (TouchLocation t1 in tcoll)
            {
                if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                {
                    if (t1.Position.X > screenposition.X)
                        directionX = 1;
                    if (t1.Position.X < screenposition.X)
                        directionX = -1;
                }
            }

            inair = !(velocity.Y >= 0 && velocity.Y <= 3);
            if (!inair || allowmoveinair)
            {
                velocity.X += directionX * power;   // Set velocity based on direction stick is pressed in

                // Set the state to indicate which way the sprite is facing, LEFT or RIGHT
                if (directionX < 0)
                    state = 1;
                else if (directionX > 0)
                    state = 0;
            }
            updatesprite(gtime, gwidth, gheight); // Update the sprites position and apply physics
        }


        // Update the sprites position by applying velocity, friction & gravity
        public void updatesprite(float gtime, int dwidth, int groundlevel)
        {
            if (visible)
            {
                // If state has changed start the new animation
                if (oldstate != state)
                {
                    previousstate = oldstate;
                    spriteanimation[state].start(screenposition, rotation, 0);   // Start the animation and set it to LOOP
                }

                velocity.Y += gravity;   // Apply gravity to the projectile
                position += velocity * (gtime / averageupdatetime);    // Move projectile using the current velocity

                // Pause the animation is the sprite stops moving
                spriteanimation[state].paused = (Math.Round(velocity.X, 1) == 0);
                
                // Check if sprite hits the ground
                if (position.Y >= groundlevel - spriteanimation[state].rect.Height / 2)
                {
                    position.Y = groundlevel - spriteanimation[state].rect.Height / 2;
                    velocity.Y = 0;
                    inair = false;
                }

                if (!inair)
                    velocity *= friction;    // Reduce projectile velocity due to air resistance
                else
                    velocity *= airresistance;    // Reduce projectile velocity due to air resistance

                // Set left and right limits
                if (position.X > dwidth - spriteanimation[state].rect.Width / 2)
                    position.X = dwidth - spriteanimation[state].rect.Width / 2;
                if (position.X < spriteanimation[state].rect.Width / 2)
                    position.X = spriteanimation[state].rect.Width / 2;

                // update the animation
                spriteanimation[state].update(gtime);
                // set a box bounding around it
                updatebox();
                oldstate = state; // Store current state

                // If an animation has finished playing reset it to the previous animation
                if (!spriteanimation[state].visible && previousstate != state && previousstate >= 0)
                {
                    state = previousstate;
                    spriteanimation[state].start(screenposition, rotation, 0);   // Start the animation
                }
            }
        }

        public void updatebox()
        {
            bbox = new BoundingBox(new Vector3(position.X - spriteanimation[state].rect.Width / 2, position.Y - spriteanimation[state].rect.Height / 2, 0),
                new Vector3(position.X + spriteanimation[state].rect.Width / 2, position.Y + spriteanimation[state].rect.Height / 2, 10));
        }

        // Make the sprite jump based on the forceapplied which is passed in
        public void jump(float forceappplied)
        {
            if (velocity.Y == 0)
            {
                // Set direction and force of the jump, taking account of the speed of the pc
                velocity.Y -= forceappplied;

                // Adjust animation states for jumping (only do this if you have seperate jump animations)
                if (numberofstates >= 3)
                {
                    if (state == 0) state = 2;
                    if (state == 1) state = 3;
                }
            }
        }

        // Draw the correct sprite animation at the current position 
        public void drawme(ref SpriteBatch sbatch)
        {
            if (visible)
                spriteanimation[state].drawme(ref sbatch, screenposition);
        }

        // Use this method to draw the sprite animation at a specified position
        public void drawme(ref SpriteBatch sbatch, Vector3 newpos)
        {
            if (visible)
                spriteanimation[state].drawme(ref sbatch, newpos);
        }
    }


    
    public static class sfunctions2d
    {
        // Function to handle collision response
        public static void cresponse(Vector3 position1, Vector3 position2, ref Vector3 velocity1, ref Vector3 velocity2, float weight1, float weight2)
        {
            // Calculate Collision Response Directions
            Vector3 x = position1 - position2;
            x.Normalize();
            Vector3 v1x = x * Vector3.Dot(x, velocity1);
            Vector3 v1y = velocity1 - v1x;
            x = -x;
            Vector3 v2x = x * Vector3.Dot(x, velocity2);
            Vector3 v2y = velocity2 - v2x;

            velocity1 = v1x * (weight1 - weight2) / (weight1 + weight2) + v2x * (2 * weight2) / (weight1 + weight2) + v1y;
            velocity2 = v1x * (2 * weight1) / (weight1 + weight2) + v2x * (weight2 - weight1) / (weight1 + weight2) + v2y;
        }

        public static Vector3 midpoint(Vector3 position1, Vector3 position2)
        {
            Vector3 middle = new Vector3(0, 0, 0);
            middle.X = (position1.X + position2.X) / 2;
            middle.Y = (position1.Y + position2.Y) / 2;
            middle.Z = (position1.Z + position2.Z) / 2;

            return middle;
        }
    }

}