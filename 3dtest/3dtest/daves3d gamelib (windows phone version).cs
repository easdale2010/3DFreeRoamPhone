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


namespace gamelib3d
{
    public class camera
    {
        public Vector3 position;
        public Vector3 lookAt;
        public float aspectratio;
        public float fov;
        public Vector2 ViewAreaAtFarPlane;
        public Vector2 ViewAreaAtFocalPlane;
        public Vector2 ViewAreaAtNearPlane;
        public Vector3 WhichWayIsUp;
        private float nearplane;
        private float farplane;
        private float focalplane;

        public Matrix getview()
        {
            return Matrix.CreateLookAt(position, lookAt, WhichWayIsUp);    // Set the position of the camera and tell it what to look at
        }

        public Matrix getproject()
        {
            return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectratio, nearPlane, farPlane);
        }

        public float farPlane
        {
            set
            {
                farplane = value;
                ViewAreaAtFarPlane = ViewAreaAt(farplane);
            }

            get
            {
                return farplane;
            }
        }

        public float focalPlane
        {
            set
            {
                focalplane = value;
                ViewAreaAtFocalPlane = ViewAreaAt(focalplane);
            }

            get
            {
                return focalplane;
            }
        }

        public float nearPlane
        {
            set
            {
                nearplane = value;
                ViewAreaAtNearPlane = ViewAreaAt(nearplane);
            }

            get
            {
                return nearplane;
            }
        }

        public Vector2 ViewAreaAt(float distance)
        {
            Vector2 area = new Vector2(0,0);
            area.Y = (int)(distance * Math.Tan(MathHelper.ToRadians(fov / 2)));
            area.X = area.Y * aspectratio;
            return area;
        }

        public Matrix FOVMatrix()
        {
            return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectratio, nearplane, farplane);
        }

        public Matrix LookMatrix()
        {
            return Matrix.CreateLookAt(position, lookAt, WhichWayIsUp);
        }

        public camera() {}

        public camera(Vector3 initialPosition, Vector3 lookat, float w_width, float w_height, int FOV, Vector3 camorient, float cameradistance, float farplanedistance)
        {
            position = initialPosition;
            lookAt = lookat;
            aspectratio = w_width / w_height;
            fov = FOV;
            WhichWayIsUp = camorient;
            nearPlane = 1f;
            farPlane = farplanedistance;
            focalPlane = cameradistance;
        }

        // Set camera for a first or third person viewpoint
        public void setFPor3P(Vector3 character_position, Vector3 character_direction, Vector3 character_velocity, float howfarbehind, float distanceinfront, float distanceabove1, float distanceabove2)
        {
            character_velocity.Y = 0;
            position = character_position - ((character_direction * howfarbehind) + character_velocity * 3);
            position.Y += distanceabove1;
            lookAt = character_position + ((character_direction * distanceinfront) + character_velocity * 1);
            lookAt.Y += distanceabove2;
            focalPlane = howfarbehind+distanceinfront;
            WhichWayIsUp = Vector3.Up;
        }

        // Set overhead view of main game character
        public void setoverhead(Vector3 character_position, float distanceabove)
        {
            lookAt = character_position;
            position = character_position;
            position.Y = distanceabove;
            focalPlane = distanceabove;
            WhichWayIsUp = Vector3.Left;
        }

        // Set camera for a side on view of the main game character
        public void setsideon(Vector3 character_position, Vector3 character_rotation, float distancefromcharacter, float distanceabove)
        {
            Vector3 camdirection = new Vector3(0, 0, 0);
            camdirection.Z = (float)(Math.Cos(character_rotation.Y + MathHelper.ToRadians(90)));
            camdirection.X = (float)(Math.Sin(character_rotation.Y + MathHelper.ToRadians(90)));
            lookAt = character_position;
            position = character_position - (camdirection * distancefromcharacter);
            position.Y += distanceabove;
            focalPlane = distancefromcharacter;
            WhichWayIsUp = Vector3.Up;
        }

    }


    // Class for 3D models that don't move
    public class staticmesh
    {
        public Model graphic;           // The 3D Model object that we are going to display.
        public Matrix[] transforms;     // Holds model transformation matrix
        public Vector3 rotation;        // Amount of rotation to apply on x,y and z
        public Vector3 position;        // Position on screen
        public float size;              // Size ratio (scale) 
        public float radius;            // Radius of 3D model
        public BoundingSphere bsphere;  // Bounding sphere
        public BoundingBox bbox;        // Bounding box
        public Vector3 bboxsize = new Vector3(0, 0, 0);        // Bounding box size
        public float weight;            // Weight
        public Boolean visible = true;  // Should object be drawn?
        public Matrix worldmatrix;

        //        public Matrix scalematrix;
        //      public Matrix translationmatrix;
        //      public Matrix rotationmatrix;


        public staticmesh() { }         // Empty constructor which is only in to stop crashes 

        // Constructor which loads 3D model and sets it up with size, position and initial rotation
        public staticmesh(ContentManager content, string modelname, float msize, Vector3 mpos, Vector3 mrot)
        {
            graphic = content.Load<Model>(modelname);                   // Load the 3D model from the ContentManager
            transforms = new Matrix[graphic.Bones.Count];               // make an array of transforms, one for each 'bone' in the 3d model
            graphic.CopyAbsoluteBoneTransformsTo(transforms);           // copy the transforms from the 3d model into this array ready for use
            size = msize;                                               // Set size 
            radius = graphic.Meshes[0].BoundingSphere.Radius * size;    // Work out the radius 
            position = mpos;                                            // Set initial position
            rotation = mrot;                                            // Set intial rotation 

            worldmatrix = Matrix.Identity * Matrix.CreateScale(size);
            worldmatrix.Translation = position;

            updateobject();
        }

        public void drawme(camera newcam, Boolean lightson)
        {
            if (visible)
                sfunctions3d.drawmodel(position, rotation, size, graphic, transforms, lightson, newcam);
        }

        public void updateobject()
        {
            if (bboxsize.X == 0)
            {
                // Create a boundingsphere around object
                bsphere = new BoundingSphere(position, radius);
            }
            else
            {
                // Create a bounding box around the 3D model
                Vector3 leftcorner = position;
                Vector3 rightcorner = position;
                leftcorner -= bboxsize;
                rightcorner += bboxsize;
                bbox = new BoundingBox(leftcorner, rightcorner);
            }
        }
    }

    
    // This is a subclass of staticmesh and inherits all it's attributes
    // We have added the necessary attributes to this class to allow us to move the model
    public class model3d : staticmesh
    {
        public Vector3 direction;       // Direction object is pointing in
        public float power;             // Amount of potential energy the object has in regards to movement
        public Vector3 velocity = Vector3.Zero;        // Velocity (Direction and speed)
        public float rotspeed;          // Speed of rotation
        public float speed;             // Actual speed object is going at
        public Vector3 oldposition;     // Position before collision occurs
        public Vector3 oldrotation;     // Rotation before collision occured
        private const float friction = 0.97f;   // Friction to apply to object
        private const float gravity = 2f;       // Force of gravity to apply
        private const float airresistance = 0.9999f;   // Amount of air resistance to apply when character is in the air
        

        public model3d() { }

        // Constructor which loads 3D model and sets it up with size, position and initial rotation
        public model3d(ContentManager content, string modelname, float msize, Vector3 mpos, Vector3 mrot, float rspeed, float mpower, float mweight)
        {
            graphic = content.Load<Model>(modelname);                   // Load the 3D model from the ContentManager
            transforms = new Matrix[graphic.Bones.Count];               // make an array of transforms, one for each 'bone' in the 3d model
            graphic.CopyAbsoluteBoneTransformsTo(transforms);           // copy the transforms from the 3d model into this array ready for use
            size = msize;                                               // Set size 
            radius = graphic.Meshes[0].BoundingSphere.Radius * size;    // Work out the radius 
            position = mpos;                                            // Set initial position
            rotation = mrot;                                            // Set intial rotation            
            rotspeed = rspeed;                                          // Set rotation speed
            power = mpower;                                             // Set default power
            weight = mweight;                                           // Set the weight of the object
            updateobject();

            worldmatrix = Matrix.Identity * Matrix.CreateScale(size);
            worldmatrix.Translation = position;
        }

        // Apply standard physics to the movement of the 3D object
        public void applymove(int groundpos)
        {
            // If 3d object is not on the ground then apply gravity
            if (position.Y > groundpos) velocity.Y -= gravity;

            // Keep 3d object from falling through ground
            if (position.Y < groundpos)
            {
                position.Y = groundpos;
                velocity.Y = 0;
            }

            // Add current velocity to the position of the ball
            position += velocity;

            // Work out the current speed object is moving at
            speed = (float)(Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z));

            // Apply friction or air resistance depending on if object is in the air
            if (velocity.Y == 0)
                velocity *= friction;   // Apply friction 
            else
                velocity *= airresistance;    // Reduce object velocity due to air resistance if object is in flight

            updateobject();
        }

        // Method to allow the user to move the object
        public void moveme(GamePadState pad, float gtime, int groundpos)
        {
            if (visible && velocity.Y == 0)
            {
                // Turn R2 based on the RIGHT thumbstick, rotation speed of the object and gametime
                rotation.Y -= pad.ThumbSticks.Right.X * rotspeed * gtime;

                if (MathHelper.ToDegrees(rotation.Y) >= 360) rotation.Y = 0;
                if (MathHelper.ToDegrees(rotation.Y) < 0) rotation.Y = MathHelper.ToRadians(359);

                // Work out the direction R2 is facing
                direction.Z = (float)(Math.Cos(rotation.Y));
                direction.X = (float)(Math.Sin(rotation.Y));

                // Work out the angle that the user has pressed the left stick in
                float botdirection = (float)Math.Atan2(-pad.ThumbSticks.Left.X, pad.ThumbSticks.Left.Y);

                // Work out the direction that the user wants to move the object in, by taking account of the current direction he is facing 
                // plus the direction he indictated on the left stick
                Vector3 stickdirection = new Vector3(0, 0, 0);
                stickdirection.Z = (float)(Math.Cos(rotation.Y + botdirection));
                stickdirection.X = (float)(Math.Sin(rotation.Y + botdirection));

                // Work out the amount of total force being applied by working out the total amount the left stick is being pressed
                float currentthrust = (float)(Math.Sqrt(Math.Pow(pad.ThumbSticks.Left.X, 2) + Math.Pow(pad.ThumbSticks.Left.Y, 2)));
                // Increase thrust based on the power factor on the object & apply gametime to even out the speed of movement based on the speed of the PC
                currentthrust *= power;

                // Apply the direction * thrust to the current velocity
                velocity += stickdirection * currentthrust * gtime;
            }

            // Apply standard movement 
            applymove(groundpos);
        }

        // Method to allow the user to move the object
        public void moveme(Vector2 pushed, float turnamount, float gtime, int groundpos)
        {
            if (visible && velocity.Y == 0)
            {
                // Turn R2 based on the RIGHT thumbstick, rotation speed of the object and gametime
                rotation.Y += turnamount * rotspeed * gtime;

                if (MathHelper.ToDegrees(rotation.Y) >= 360) rotation.Y = 0;
                if (MathHelper.ToDegrees(rotation.Y) < 0) rotation.Y = MathHelper.ToRadians(359);

                // Work out the direction R2 is facing
                direction.Z = (float)(Math.Cos(rotation.Y));
                direction.X = (float)(Math.Sin(rotation.Y));

                // Calculate the angle of direction that the user has pressed the left stick in
                float botdirection = (float)Math.Atan2(pushed.X, pushed.Y);

                // Calculate the direction that the robot should move based on the way he is facing plus the user input
                Vector3 stickdirection = new Vector3(0, 0, 0);
                stickdirection.Z = (float)(Math.Cos(rotation.Y + botdirection));
                stickdirection.X = (float)(Math.Sin(rotation.Y + botdirection));

                // Work out the thrust based on user input
                float currentthrust = (float)(Math.Sqrt(Math.Pow(pushed.X, 2) + Math.Pow(pushed.Y, 2)));
                // Increase thrust based on the power factor on the object & apply gametime to even out the speed of movement based on the speed of the PC
                currentthrust *= power;

                // Apply the direction * thrust to the current velocity
                velocity += stickdirection * currentthrust * gtime;
            }

            // Apply standard movement 
            applymove(groundpos);
        }


        // Make the sprite jump based on the forceapplied which is passed in
        public void jump(float forceappplied)
        {
            if (visible && velocity.Y == 0)
            {
                // Set direction and force of the jump, taking account of the speed of the pc
                velocity.Y += forceappplied;
            }
        }

    }


    public static class sfunctions3d
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

        // Reset graphics device for 3D drawing
        public static void resetgraphics(GraphicsDevice graphics)
        {
            // These lines reset the graphics device for drawing 3D
            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;
            graphics.SamplerStates[0] = SamplerState.LinearWrap;
        }

        // This method draws a 3D model
        public static void drawmesh(Vector3 position, Vector3 rotation, float scale, Model graphic, Matrix[] transforms, Boolean lightson, camera newcamera)
        {
            //Quaternion rot = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.Up, rotation.Y) * Quaternion.CreateFromAxisAngle(Vector3.Right, rotation.X)  * Quaternion.CreateFromAxisAngle(Vector3.Backward, rotation.Z));
            foreach (ModelMesh mesh in graphic.Meshes)               // loop through the mesh in the 3d model, drawing each one in turn.
            {
                foreach (BasicEffect effect in mesh.Effects)                // This loop then goes through every effect in each mesh.
                {
                    if (lightson) effect.EnableDefaultLighting();           // Enables default lighting when lightson==TRUE, this can do funny things with textured on 3D models.
                    effect.PreferPerPixelLighting = true;                   // Makes it shiner and reflects light better
                    effect.World = transforms[mesh.ParentBone.Index];       // begin dealing with transforms to render the object into the game world
                    effect.World *= Matrix.CreateScale(scale);              // scale the mesh to the right size
                    //effect.World *= Matrix.CreateRotationX(rotation.X);     // rotate the mesh
                    //effect.World *= Matrix.CreateRotationY(rotation.Y);     // rotate the mesh
                    //effect.World *= Matrix.CreateRotationZ(rotation.Z);     // rotate the mesh
                    //                    effect.World *= Matrix.CreateFromAxisAngle(rotaxis, rotamount);
                    //                    effect.World *= Matrix.CreateFromQuaternion(rot);     // Rotate the mesh using Quaternions (This may work better if you are rotating multiple axes)
                    effect.World *= Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z); // rotate the mesh
                    effect.World *= Matrix.CreateTranslation(position);     // position the mesh in the game world

                    effect.View = Matrix.CreateLookAt(newcamera.position, newcamera.lookAt, newcamera.WhichWayIsUp);    // Set the position of the camera and tell it what to look at

                    // Sets the FOV (Field of View) of the camera. The first paramter is the angle for the FOV, the 2nd is the aspect ratio of your game, 
                    // the 3rd is the nearplane distance from the camera and the last paramter is the farplane distance from the camera.
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(newcamera.fov), newcamera.aspectratio, newcamera.nearPlane, newcamera.farPlane);
                }
                mesh.Draw(); // draw the current mesh using the effects.
            }
        }

        // This method draws a 3D model with its position and orientation set by the worldmatrix being passed in
        public static void drawmesh(Matrix worldmatrix, Model graphic, Matrix[] transforms, Boolean lightson, camera newcamera)
        {
            foreach (ModelMesh mesh in graphic.Meshes)               // loop through the mesh in the 3d model, drawing each one in turn.
            {
                foreach (BasicEffect effect in mesh.Effects)                // This loop then goes through every effect in each mesh.
                {
                    if (lightson) effect.EnableDefaultLighting();           // Enables default lighting when lightson==TRUE, this can do funny things with textured on 3D models.
                    effect.PreferPerPixelLighting = true;                   // Makes it shiner and reflects light better
                    effect.World = transforms[mesh.ParentBone.Index];       // begin dealing with transforms to render the object into the game world
                    effect.World *= worldmatrix;
                    effect.View = Matrix.CreateLookAt(newcamera.position, newcamera.lookAt, newcamera.WhichWayIsUp);    // Set the position of the camera and tell it what to look at
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(newcamera.fov), newcamera.aspectratio, newcamera.nearPlane, newcamera.farPlane); // Set Field of View for camera
                }
                mesh.Draw(); // draw the current mesh using the effects.
            }
        }

        // Draw 3D model using Quaternions
        public static void drawmodel(Vector3 position, Vector3 rotation, float scale, Model modelToDraw, Matrix[] transforms, Boolean lightson, camera newcamera)
        {
            //            Matrix[] transforms = new Matrix[modelToDraw.Bones.Count];
            //            modelToDraw.CopyAbsoluteBoneTransformsTo(transforms);   // get pos of bones in model

            foreach (ModelMesh mesh in modelToDraw.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (lightson) effect.EnableDefaultLighting();           // Enables default lighting when lightson==TRUE, this can do funny things with textured on 3D models.
                    effect.PreferPerPixelLighting = true;                   // Makes it shiner and reflects light better
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateFromQuaternion(
                            Quaternion.CreateFromAxisAngle(Vector3.Right, rotation.X) *
                            Quaternion.CreateFromAxisAngle(Vector3.Backward, rotation.Z) *
                            Quaternion.CreateFromAxisAngle(Vector3.Up, rotation.Y)) *
                        Matrix.CreateScale(scale) *
                        Matrix.CreateTranslation(position);

                    // Set the position of the camera and tell it what to look at
                    effect.View = Matrix.CreateLookAt(newcamera.position, newcamera.lookAt, newcamera.WhichWayIsUp);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(newcamera.fov), newcamera.aspectratio, newcamera.nearPlane, newcamera.farPlane);
                }
                mesh.Draw();
            }
        }



    }

}