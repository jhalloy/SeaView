# SeaView

### Description

This project is the 3D visualization of ocean currents contained in the NASA DYAMOND LLC2160 Dataset. This is part of the [IEEE 2026 SciVis Contest](https://sciviscontest2026.github.io/home/) task 3. Task 3 of the contest originally pertained to the ECCO LLC4320 dataset. However, the contest organizers let us know that the u and v fields (horizontal velocities) were not yet available. They recommended we instead use the LLC2160 dataset since it is essentially the same data, just at a lower resolution.

**llc2160_data_exploration.ipynb**: notebook used to collect statistics and create graphs from the LLC2160 dataset.

**llc2160_matplotlib_3d.ipynb**: notebook which creates a 3D visualization of a reduced version of the LLC2160 dataset. This is meant to be a proof-of-concept for our Unity 3D visualization.

**llc2160_server.py**: Python script which runs a Flask server to retrieve the LLC2160 data with OpenVisus and then relay it to Unity. It accepts HTTP GET requests from Unity which contain the parameters for the data request. This was required because there is no OpenVisus C# implementation (the Unity code is in C#).

**depth_levels.txt**: contains the depth levels for each of the 90 levels in the LLC2160 dataset. Each line contains the depth below sea level in meters. This metadata was retrieved from the [SciVis Contest Data page](https://sciviscontest2026.github.io/data/home).

**environment.yml**: contains the necessary packages needed to run the Python code. This file was inspired by [https://github.com/sci-visus/Openvisus-NASA-Dashboard/blob/main/environment.yml](https://github.com/sci-visus/Openvisus-NASA-Dashboard/blob/main/environment.yml), but was modified to include the packages we need.

### Installation instructions

**For Python code**: The required packages are in `environment.yml`. This file can be used to create a conda environment with `conda env create -f environment.yml`.

**For Unity**: First, the Unity Hub must be installed. Instructions are available for this on Unity's website: [Install the Unity Hub](https://docs.unity3d.com/hub/manual/InstallHub.html). 
Open the Unity Hub and select "Add" -> "Add project from disk". Locate the `SeaView Unity` folder and select open. This will add the project to the Unity Hub. The project has the Unity editor version embedded in it, which is version 6000.0.45f1. If you do not have this Unity editor installed yet, the Unity Hub will prompt you to install the editor. Click on the "Download" or "Install Editor" button and wait for the installation to complete. Once it is done, the project is ready to be opened.

### Use

**To run the jupyter notebooks**: activate the conda environment you installed with `conda activate OpenVisus-NASA`. Then, open Jupyter Lab with `jupyter-lab`. Ensure the Python kernel you are using for these notebooks is set to the conda OpenVisus-NASA environment.

**To run the Unity visualization**: open a new terminal and activate the conda environment you installed with `conda activate OpenVisus-NASA`. Run the server code with `python llc2160_server.py`. Open the Unity Hub and then select the SeaView Unity project to open it. Once it is open, check that you have the `Main` scene loaded. If it is not, navigate to `Assets/Scenes` in the Project tab and double-click `Main`. Click Play at the top center of the page. You now should be able to see and interact with the visualization.

### Contact information

Julian Halloy - jhalloy@vols.utk.edu

Mason Stott - mstott3@vols.utk.edu

### Acknowledgements

The SciVis Contest organizers for providing the data and tasks to complete.
