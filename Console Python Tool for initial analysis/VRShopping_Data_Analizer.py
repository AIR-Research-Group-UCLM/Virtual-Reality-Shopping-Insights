import pandas as pd
import numpy as np
import os
import sys

from vr_idt.vr_idt import classify_fixations
from Eye_Tracking_Analyzer import generate_statistics_report_ET
from ProductInteraction_Analyzer import generate_pdf_report
from Navigation_data_analyzer_v2 import generate_report
from distances import compute_movement

VALID_SECTIONS = ["Food", "Technology", "Decoration", "Toys", "Fashion"]

def sanitize_dataframe(df):

    sanitized_df = df.drop_duplicates(subset=['Frame'])
    return sanitized_df

def find_most_recent_subdirectory(directory):
    
	subdirs = [os.path.join(directory, d) for d in os.listdir(directory) if os.path.isdir(os.path.join(directory, d))]
	if not subdirs:
		return None
	recent_subdirectory = max(subdirs, key=os.path.getmtime)
	return os.path.basename(recent_subdirectory)

def get_all_subdirectories(directory):
    return [name for name in os.listdir(directory) if os.path.isdir(os.path.join(directory, name))]

def load_every_csv_in_directory_in_dictionary(directory):
	dataframes = {}
	for item in os.listdir(directory):
		item_path = os.path.join(directory, item)
		if os.path.isfile(item_path) and item.endswith(".csv"):
			dataframes[item] = pd.read_csv(item_path)
			dataframes[item].columns = dataframes[item].columns.str.strip()
			dataframes[item].Name = item
	return dataframes

def generate_csv_with_fixations(dataframes_dict, subdir_containing_csvs, df, name, min_duration, max_angle, min_freq, **col_name_map):
	
	df = classify_fixations(df, min_duration, max_angle, min_freq, **col_name_map)
	head_and_hands_df = dataframes_dict["HeadHandsDataBigEnvironment.csv"]
	merged_by_zone = pd.merge(df, head_and_hands_df[['Frame', 'Zone']], on='Frame', how='left')
	final_df = merged_by_zone.copy()
	final_df.to_csv('./{0}_withFixations.csv'.format(name), index=False)
	return df

def segment_in_zones(dataframes_dict, answer='N', shelf_limit=0.15, adjacent_limit=0.325, near_limit=0.55):
    tp_df = dataframes_dict["TeleportDataBigEnvironment.csv"]
    
    first_tp_frame = tp_df[tp_df['WasTP'] == True]['Frame'].min()

    df = dataframes_dict["HeadHandsDataBigEnvironment.csv"]
    
    if 'Zone' in df.columns:
        print('CSV file is already segmented in ZOIs.')
        sys.exit()
    
    df['Zone'] = ''
    
    df.loc[df['Frame'] < first_tp_frame, 'Zone'] = 'Start'
    
    for i, row in df.iterrows():
        if row['Frame'] < first_tp_frame:
            continue
        if row['Distance'] <= shelf_limit:
            df.at[i, 'Zone'] = 'Shelf'
        elif shelf_limit < row['Distance'] <= adjacent_limit:
            df.at[i, 'Zone'] = 'Adjacent'
        elif adjacent_limit < row['Distance'] <= near_limit:
            df.at[i, 'Zone'] = 'Near'
        else:
            df.at[i, 'Zone'] = 'Far'
    
    return df
      
def main():
	
	FILENAMES = ["EyeTrackerData-ProductsBigEnvironment.csv", "EyeTrackerData-AOIBigEnvironment.csv", "HeadHandsDataBigEnvironment.csv", 
			 "ProductInteractionDataBigEnvironment.csv", "ShelvesDataManager.csv", "ShoppingCartDataBigEnvironment.csv", 
			 "TeleportDataBigEnvironment.csv", "TurningsBigEnvironment.csv"]

	col_name_map = {
		"time": "Timestamp",
		"gaze_world_x": "RCHit_x",
		"gaze_world_y": "RCHit_y",
		"gaze_world_z": "RCHit_z",
		"head_pos_x": "HMD_x",
		"head_pos_y": "HMD_y",
		"head_pos_z": "HMD_z"
	}

	directory = None
	recent_subdirectory = None
	subdir_containing_csvs = None
	dataframes_dict = None
	df = None

	# time_th=0.25, disp_th=1, freq_th=30
	min_duration = 0.15
	max_angle = 1.5
	min_freq = 30

	ascii_art = """
          _    _ _____ _     _____ ________  ________   _____ _____   _   _______ _____ _____   _____ _____ ___ _____ _____ _____ _____ _____ _____ _____  ___  ______________ _   _ _     _____           
         | |  | |  ___| |   /  __ |  _  |  \/  |  ___| |_   _|  _  | | | | | ___ /  ___|_   _| /  ___|_   _/ _ |_   _|_   _/  ___|_   _|_   _/  __ /  ___| |  \/  |  _  |  _  | | | | |   |  ___|          
 ______  | |  | | |__ | |   | /  \| | | | .  . | |__     | | | | | | | | | | |_/ \ `--.  | |   \ `--.  | |/ /_\ \| |   | | \ `--.  | |   | | | /  \\ `--.  | .  . | | | | | | | | | | |   | |__    ______  
|______| | |/\| |  __|| |   | |   | | | | |\/| |  __|    | | | | | | | | | |    / `--. \ | |    `--. \ | ||  _  || |   | |  `--. \ | |   | | | |    `--. \ | |\/| | | | | | | | | | | |   |  __|  |______| 
         \  /\  | |___| |___| \__/\ \_/ | |  | | |___    | | \ \_/ / \ \_/ | |\ \/\__/ /_| |_  /\__/ / | || | | || |  _| |_/\__/ / | |  _| |_| \__//\__/ / | |  | \ \_/ | |/ /| |_| | |___| |___           
          \/  \/\____/\_____/\____/\___/\_|  |_\____/    \_/  \___/   \___/\_| \_\____/ \___/  \____/  \_/\_| |_/\_/  \___/\____/  \_/  \___/ \____\____/  \_|  |_/\___/|___/  \___/\_____\____/           
                                                                                                                                                                                                           
                                                                                                                                                                                                           
 _            ______      _                  _____                     _             _             _                                                                                                       
| |           | ___ \    | |                |  __ \                   | |           | |           | |                                                                                                      
| |__  _   _  | |_/ _   _| |__   ___ _ __   | |  \/_ __ __ _ _ __   __| | ___    ___| |_      __ _| |                                                                                                      
| '_ \| | | | |    | | | | '_ \ / _ | '_ \  | | __| '__/ _` | '_ \ / _` |/ _ \  / _ | __|    / _` | |                                                                                                      
| |_) | |_| | | |\ | |_| | |_) |  __| | | | | |_\ | | | (_| | | | | (_| |  __/ |  __| |_ _  | (_| | |                                                                                                      
|_.__/ \__, | \_| \_\__,_|_.__/ \___|_| |_|  \____|_|  \__,_|_| |_|\__,_|\___|  \___|\__(_)  \__,_|_|                                                                                                      
        __/ |                                                                                                                                                                                              
       |___/
	   """
	print(ascii_art)
	if(not os.path.exists(os.path.join(directory, "reports"))):
		os.mkdir(os.path.join(directory, "reports"))
                                                                                                                                                                                          
	directory = input("Please, provide the directory of the execution you want to analyse: ")
	is_segment_selected = ""
	while is_segment_selected.upper() not in ['Y', 'N']:
		is_segment_selected = input("Please, indicate if you want to segment the data in zones based on distance of the player to the shelves (Y/N): ")
	
	choice = input("Do you want to analyze a single session or a user's history? (S/H): ").strip().lower()

	if choice == 's':

		dataframes_dict = load_every_csv_in_directory_in_dictionary(directory)
		print(f"Data loaded from the most recent session: {directory}")
		for key, value in dataframes_dict.items():
			print(f"{key}: {len(value)} rows")
	elif choice == 'h' or choice == 'n' or choice == 'no':
		# Analyzing user's history
		# all_subdirectories = get_all_subdirectories(directory)
		# print("All subdirectories (user's history):")
		# print(all_subdirectories)
		print("Work in progress feature. Please, analyze a single session.")
		exit()
	else:
		print("Invalid input. Please enter 'S' for single session or 'H' for user's history.")

	if(is_segment_selected.upper() == 'Y'):	
		is_default_values_selected = ""
	while is_default_values_selected.upper() not in ['Y', 'N']:
		is_default_values_selected = input("Do you want to use the default values for the distance limits? (Y/N): ")
	if (is_default_values_selected.upper() == 'N'):
		shelf_limit = input("Enter the shelf distance limit in meters: ")
		adjacent_limit = input("Enter the adjacent distance limit in meters: ")
		near_limit = input("Enter the near distance limit in meters: ")
		try:
			shelf_limit = float(shelf_limit)
			adjacent_limit = float(adjacent_limit)
			near_limit = float(near_limit)
		except ValueError:
			print("Invalid input. Distance limits must be numeric values.")
			sys.exit()
		df = segment_in_zones(dataframes_dict, shelf_limit=shelf_limit, adjacent_limit=adjacent_limit, near_limit=near_limit)
	else:
		df = segment_in_zones(dataframes_dict)
	
	df_segmented_and_movement = compute_movement(df, 0.01)
	df_segmented_and_movement.to_csv('./HeadHandsDataBigEnvironment_segmented.csv', index=False)
	dataframes_dict["HeadHandsDataBigEnvironment_segmented.csv"] = df_segmented_and_movement

	
	generate_csv_with_fixations(dataframes_dict, subdir_containing_csvs, dataframes_dict["EyeTrackerData-ProductsBigEnvironment.csv"], "EyeTrackerData-ProductsBigEnvironment",
								min_duration, max_angle, min_freq, **col_name_map)
	generate_csv_with_fixations(dataframes_dict, subdir_containing_csvs, dataframes_dict["EyeTrackerData-AOIBigEnvironment.csv"], "EyeTrackerData-AOIBigEnvironment", 
								min_duration, max_angle, min_freq, **col_name_map)

	# Generate the PDF report of ET data
	dataframes_dict["EyeTrackerData-AOIBigEnvironment_withFixations.csv"] = pd.read_csv('./EyeTrackerData-AOIBigEnvironment_withFixations.csv')
	eye_tracking_data_aoi_df = sanitize_dataframe(dataframes_dict["EyeTrackerData-AOIBigEnvironment_withFixations.csv"])
	generate_statistics_report_ET(eye_tracking_data_aoi_df, "./reports/VRSI_EyeTrackingAOIs_Report.pdf")

	# Generate the PDF report of Product Interaction data
	generate_pdf_report()
	
	# Generate the PDF report of Navigation data
	generate_report('./reports/VRSI_Navigation_Report.pdf', dataframes_dict["HeadHandsDataBigEnvironment_segmented.csv"], 
				 dataframes_dict["TeleportDataBigEnvironment"], VALID_SECTIONS)

if __name__ == "__main__":
	main()