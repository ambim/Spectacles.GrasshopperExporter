﻿//The MIT License (MIT)

//Copyright (c) 2015 Thornton Tomasetti

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using Spectacles.GrasshopperExporter.Properties;


namespace Spectacles.GrasshopperExporter
{
    public class Spectacles_Views : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Spectacles_SceneCompiler class.
        /// </summary>
        public Spectacles_Views()
            : base("Spectacles_Views", "Spectacles_Views",
                "Compiles the views selected to be exported",
                "TT Toolbox", "Spectacles")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Eye Position", "E", "Position of the viewer", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "T", "Position of the target", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Name of this camera", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("View", "Ve", "View Element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Point3d eye = new Point3d();
            Point3d target = new Point3d();
            string name = "";


            //get user inputs
            //user should be able to create a scene contianing only lines, or only meshes, or both.  All geo and material inputs will be optional, and we'll run some defense.
            if (!DA.GetData(0, ref eye)) return;
            if (!DA.GetData(1, ref target)) return;
            if (!DA.GetData(2, ref name)) return;

            try
            {
                //create json from lists of json:
                string outJSON = pointJSON(eye, target,name);
                outJSON = outJSON.Replace("OOO", "object");
                
                Element e = new Element(outJSON, SpectaclesElementType.Camera);
                DA.SetData(0, e);
            }
            catch (Exception)
            {
                return;
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.CAMERA;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{20719351-0b0c-4933-ad77-9790d09ee6e8}"); }
        }

        private string pointJSON(Point3d E, Point3d T, string name)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //top level properties
            jason.uuid = Guid.NewGuid();
            jason.name = name;

            jason.eye = new ExpandoObject();
            //populate data object properties
            jason.eye.X = Math.Round(E.X * -1, 5);
            jason.eye.Y = Math.Round(E.Z, 5);
            jason.eye.Z = Math.Round(E.Y, 5);
            
            jason.target = new ExpandoObject();
            //populate data object properties
            jason.target.X = Math.Round(T.X * -1, 5);
            jason.target.Y = Math.Round(T.Z, 5);
            jason.target.Z = Math.Round(T.Y, 5);
            
            //return
            return JsonConvert.SerializeObject(jason);
        }
    }
}