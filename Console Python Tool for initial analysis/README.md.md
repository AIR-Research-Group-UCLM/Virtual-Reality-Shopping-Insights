# Behavior Statistics analyzer tool
This folder contains a set of Python scripts designed to analyze data from VR shopping sessions. 
The scripts include eye-tracking analysis, product interaction analysis, navigation analysis, and more. 

## Requirements 
Make sure to have the following dependencies installed before running the scripts: 
- matplotlib==3.8.3 
- numpy==2.0.0
 - pandas==2.2.2 
 - PyPDF2==3.0.1 
 - reportlab==4.2.0 
 - seaborn==0.13.2
 -  vr_idt==0.0.5 
You can install all dependencies by running the following command: 
```bash 
pip install -r requirements.txt
```

## Project Files

-   `VRShopping_Data_Analizer.py`: Main file to execute the data analysis.
-   `Eye_Tracking_Analyzer.py`: Script to generate statistics and graphs based on eye-tracking data.
-   `Navigation_data_analyzer_v2.py`: Script to analyze navigation data and generate reports.
-   `Positional_data_analyzer.py`: Script to analyze positional data.
-   `ProductInteraction_Analyzer.py`: Script to analyze product interactions.
-   `distances.py`: Contains helper functions to calculate distances and movements.

## Folder Structure

Ensure your folder structure looks like this:

project-root/ 
│ 
├── sample_data/ 
│ ├── ShelvesDataBigEnvironment.csv 
│ ├── HeadHandsDataBigEnvironment.csv 
│ ├── ProductInteractionDataBigEnvironment.csv
│ ├── ShoppingCartDataBigEnvironment.csv  
│ └── ... 
├── reports/ 
│ └── (Generated reports will be saved here. Sample reports will be located in this folder) 
├── VRShopping_Data_Analizer.py 
├── Eye_Tracking_Analyzer.py 
├── Navigation_data_analyzer_v2.py 
├── Positional_data_analyzer.py 
├── ProductInteraction_Analyzer.py 
├── distances.py 
└── requirements.txt

The folder in this repository contains sample data collected from an individual who executed the application. Also, you can see examples of the generated PDF reports.
The generated csv files with fixations and segmentation of the virtual environments are places in the same level as the .py files.

## Execution

To run the data analysis, follow these steps:

1.  Clone the repository to your local machine.
2.  Install the dependencies using `pip install -r requirements.txt`.
3.  Place your CSV data files from the desired execution in the `sample_data/` folder for easier localization (optional).
4.  Run the main script `VRShopping_Data_Analizer.py`: 
```bash
python VRShopping_Data_Analizer.py
```
Currently, the tool only supports the analysis of **single sessions**. Bear it in mind when selecting the options in the command line tool. 
Moreover, we recommend to segment the head and hands data file with the default distances. However, if your virtual environment requires other distances, feel free to explore the most suitable segmentation, taking into account the default ones provided from state-of-the-art works. There are some constants, such as **VALID_SECTIONS**, that you might change according your VR shopping environment.

