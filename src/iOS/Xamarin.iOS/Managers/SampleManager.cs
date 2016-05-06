﻿// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ArcGISRuntimeXamarin.Models;

namespace ArcGISRuntimeXamarin.Managers
{   
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public class SampleManager
    {
        private Assembly _samplesAssembly;
        private SampleStructureMap _sampleStructureMap;

        #region Constructor and unique instance management

        // Private constructor
        private SampleManager() { }

        // Static initialization of the unique instance 
        private static readonly SampleManager SingleInstance = new SampleManager();

        public static SampleManager Current
        {
            get { return SingleInstance; }
        }

        public async Task InitializeAsync()
        {
            //TODO Used for Removing samples
            // string filename = Path.GetFileName(GetType().Assembly.Location);
            //_samplesAssembly = Assembly.Load(filename);

            await CreateAllAsync();
            // RemoveEmptySamples();
        }

        #endregion // Constructor and unique instance management

        /// <summary>
        /// Gets or sets selected sample.
        /// </summary>
        public SampleModel SelectedSample { get; set; }

        /// <summary>
        /// Gets featured samples.
        /// </summary>
        /// <returns></returns>
        public List<FeaturedModel> GetFeaturedSamples()
        {
            return _sampleStructureMap.Featured;
        }

        /// <summary>
        /// Gets all samples as a tree.
        /// </summary>
        /// <returns>Return all categories, subcategories and samples.</returns>
        public List<TreeItem> GetSamplesAsTree()
        {
            var categories = new List<TreeItem>();
            try
            {

                foreach (var category in _sampleStructureMap.Categories)
                {
                    var categoryItem = new TreeItem();
                    categoryItem.Name = category.Name;

                    foreach (var subCategory in category.SubCategories)
                    {
                        if (subCategory.ShowGroup)
                        {
                            var subCategoryItem = new TreeItem() { Name = subCategory.Name };
                            categoryItem.Items.Add(subCategoryItem);

                            if (subCategory.Samples != null)
                                foreach (var sample in subCategory.Samples)
                                    subCategoryItem.Items.Add(sample);
                        }
                        else
                        {
                            foreach (var sample in subCategory.Samples)
                                categoryItem.Items.Add(sample);
                        }
                    }

                    categories.Add(categoryItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return categories;
        }

        /// <summary>
        /// Returns a Stream based on the individual sample metadata.json file. 
        /// </summary>
        /// <param name="path">String path to the metadata file.</param>
        /// <returns>Metadata.json stream</returns>
        public Stream GetMetadataManifest(string path)
        {
            var metadataPath = GetType().Assembly.GetManifestResourceStream("ArcGISRuntimeXamarin." + path + ".metadata.json");

            return metadataPath;
        }

        /// <summary>
        /// Creates whole sample structure.
        /// </summary>
        private async Task CreateAllAsync()
        {
            // You can no longer check to see if groups.json exists on disk here. You have to 
            // open it and verify that it isn't null.           
            Stream groupsJson = GetType().Assembly.GetManifestResourceStream("ArcGISRuntimeXamarin.groups.json");
            try
            {
                await Task.Run(() =>
                {
                    if (groupsJson == null)
                        throw new NotImplementedException("groups.json file cannot be opened");
                    _sampleStructureMap = SampleStructureMap.Create(groupsJson);
                });
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                throw; //TODO
            }
            catch (DirectoryNotFoundException e)
            {
                throw; //TODO
            }
            catch (Exception e)
            {
                throw; //TODO
            }
        }
    }
}
