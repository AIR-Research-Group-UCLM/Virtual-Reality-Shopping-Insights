import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import seaborn as sns
import numpy as np
from matplotlib.backends.backend_pdf import PdfPages

#TODO: redondear a 2 decimales todos los valores de tabla

def remove_outliers(df, column):
    """
    Removes outliers from a specified column in the DataFrame using the z-score method.

    Parameters:
    df (pandas.DataFrame): The DataFrame containing the data.
    column (str): The column name from which to remove outliers.

    Returns:
    pandas.DataFrame: The DataFrame with outliers removed.
    """
    mean = df[column].mean()
    std = df[column].std()
    z_score = (df[column] - mean) / std
    return df[z_score.abs() < 3]

def count_stops_and_moves(data):
    """
    Counts the number of 'Stop' and 'Move' events in the DataFrame.

    Parameters:
    data (pandas.DataFrame): The DataFrame containing 'Status' column with 'Stop' and 'Move' values.

    Returns:
    tuple: A tuple containing the counts of 'Stop' and 'Move' events.
    """
    data['Shift_Status'] = data['Status'].shift(1)
    data['New_Group'] = data['Status'] != data['Shift_Status']
    data['Group_ID'] = data['New_Group'].cumsum()

    group_counts = data.groupby(['Group_ID', 'Status']).size().reset_index(name='Count')
    stop_counts = group_counts[group_counts['Status'] == 'Stop'].shape[0]
    move_counts = group_counts[group_counts['Status'] == 'Move'].shape[0]

    return stop_counts, move_counts

def calculate_mean_velocity(head_hands_data):
    """
    Computes the mean and standard deviation of velocity by zone.

    Parameters:
    head_hands_data (pandas.DataFrame): The DataFrame containing head and hands tracking data.

    Returns:
    pandas.DataFrame: A DataFrame with mean and standard deviation of velocity by zone.
    """
    head_hands_data['Delta_x'] = head_hands_data['HMD_x'].diff().fillna(0)
    head_hands_data['Delta_z'] = head_hands_data['HMD_z'].diff().fillna(0)
    head_hands_data['Delta_t'] = head_hands_data['Timestamp'].diff().fillna(0)
    
    head_hands_data['Velocity'] = np.sqrt(head_hands_data['Delta_x']**2 + head_hands_data['Delta_z']**2) / head_hands_data['Delta_t']
    head_hands_data.loc[head_hands_data['Delta_t'] == 0, 'Velocity'] = 0

    # Remove outliers
    head_hands_data = remove_outliers(head_hands_data, 'Velocity')
    
    mean_std_velocity_by_zone = head_hands_data.groupby('Zone')['Velocity'].agg(['mean', 'std']).reset_index()
    mean_std_velocity_by_zone.columns = ['Zone', 'Mean_Velocity', 'Std_Velocity']
    
    return mean_std_velocity_by_zone

def label_sections(teleport_data, valid_sections):
    """
    Labels sections in the teleport data and determines the current section for each row.
    Currently, it only works if you name the Teleport Hotspots as TP_SectionName_Whatever in your Unity Scene.

    Parameters:
    teleport_data (pandas.DataFrame): The DataFrame containing teleport data.
    valid_sections (list): List of valid sections.

    Returns:
    pandas.DataFrame: The DataFrame with sections labeled and current section determined.
    """
    teleport_data['Section'] = teleport_data['TPHotspot'].str.extract(r'TP_([A-Za-z]+)', expand=False)
    teleport_data['Section'] = teleport_data['Section'].str.replace(r'\d+', '', regex=True)
    teleport_data['Section'] = teleport_data['Section'].apply(lambda x: x if x in valid_sections else 'NIAS')
    teleport_data['WasTP'] = teleport_data['WasTP'].apply(lambda x: True if x == 'True' else False)

    teleport_data['Current_Section'] = 'NIAS'
    current_section = 'NIAS'
    
    for i, row in teleport_data.iterrows():
        if row['WasTP']:
            current_section = row['Section']
        teleport_data.at[i, 'Current_Section'] = current_section

    return teleport_data

def update_head_hands_data_sections(head_hands_data, teleport_data):
    """
    Updates the head and hands data with section labels based on teleport data.

    Parameters:
    head_hands_data (pandas.DataFrame): The DataFrame containing head and hands tracking data.
    teleport_data (pandas.DataFrame): The DataFrame containing teleport data.

    Returns:
    pandas.DataFrame: The updated head and hands data with section labels.
    """
    head_hands_data['Section'] = 'NIAS'
    current_section = 'NIAS'
    
    for i, row in head_hands_data.iterrows():
        relevant_teleport = teleport_data[teleport_data['Timestamp'] <= row['Timestamp']].tail(1)
        if not relevant_teleport.empty:
            current_section = relevant_teleport.iloc[0]['Current_Section']
        head_hands_data.at[i, 'Section'] = current_section

    return head_hands_data

def plot_user_presence_with_sections(head_hands_data, teleport_data, valid_sections, pdf):
    """
    Plots user presence over time with section changes and teleport events.

    Parameters:
    head_hands_data (pandas.DataFrame): The DataFrame containing head and hands tracking data.
    teleport_data (pandas.DataFrame): The DataFrame containing teleport data.
    valid_sections (list): List of valid sections.
    pdf (PdfPages): The PDF object to save the plot.
    """
    teleport_data = label_sections(teleport_data, valid_sections)
    head_hands_data = update_head_hands_data_sections(head_hands_data, teleport_data)

    plt.figure(figsize=(14, 7))
    unique_sections = ['NIAS'] + valid_sections
    section_y_positions = {section: i for i, section in enumerate(unique_sections)}
    head_hands_data['Y_Position'] = head_hands_data['Section'].map(section_y_positions)

    for section in unique_sections:
        section_data = head_hands_data[head_hands_data['Section'] == section]
        plt.scatter(section_data['Timestamp'], section_data['Y_Position'], s=10, label=section, color=plt.cm.tab20(section_y_positions[section]))

    teleport_events = teleport_data[teleport_data['WasTP'] == True]
    for _, row in teleport_events.iterrows():
        plt.axvline(x=row['Timestamp'], color='red', linestyle='--', linewidth=1)

    plt.xlabel('Timestamp')
    plt.ylabel('Sections')
    plt.yticks(range(len(unique_sections)), unique_sections)
    plt.title('User Presence Over Time with Section Changes and Teleport Events')
    plt.legend(loc='upper right', title="Sections")
    pdf.savefig()
    plt.close()

def plot_bubble_plot(head_hands_data, teleport_data, valid_sections, pdf):
    """
    Plots a bubble plot showing user presence in sections over time with zone sizes.

    Parameters:
    head_hands_data (pandas.DataFrame): The DataFrame containing head and hands tracking data.
    teleport_data (pandas.DataFrame): The DataFrame containing teleport data.
    valid_sections (list): List of valid sections.
    pdf (PdfPages): The PDF object to save the plot.
    """
    teleport_data = label_sections(teleport_data, valid_sections)
    head_hands_data = update_head_hands_data_sections(head_hands_data, teleport_data)

    head_hands_data['Time_Delta'] = head_hands_data['Timestamp'].diff().fillna(0)
    head_hands_data['Zone'] = head_hands_data['Zone'].astype(str)
    zone_sizes = head_hands_data['Zone'].map({'Far': 1, 'Shelf': 2, 'Adjacent': 3, 'Near': 4})
    zone_colors = head_hands_data['Zone'].map({'Far': 'blue', 'Shelf': 'green', 'Adjacent': 'orange', 'Near': 'red'})

    plt.figure(figsize=(14, 7))
    for zone, color in {'Far': 'blue', 'Shelf': 'green', 'Adjacent': 'orange', 'Near': 'red'}.items():
        zone_data = head_hands_data[head_hands_data['Zone'] == zone]
        plt.scatter(zone_data['Timestamp'], zone_data['Section'], s=zone_sizes[zone_data.index]*100, color=color, alpha=0.6, edgecolors='w', linewidth=0.5, label=zone)

    teleport_events = teleport_data[teleport_data['WasTP'] == True]
    for _, row in teleport_events.iterrows():
        plt.axvline(x=row['Timestamp'], color='red', linestyle='--', linewidth=1)

    handles, labels = plt.gca().get_legend_handles_labels()
    unique_labels = dict(zip(labels, handles))
    plt.legend(unique_labels.values(), unique_labels.keys(), title="Zone Size", loc='upper right')

    plt.xlabel('Timestamp')
    plt.ylabel('Sections')
    plt.title('User Presence in Sections Over Time with Zone Sizes')
    pdf.savefig()
    plt.close()

def save_metrics_table_to_pdf(metrics_df, pdf):
    """
    Saves a metrics table to a PDF.

    Parameters:
    metrics_df (pandas.DataFrame): The DataFrame containing the metrics.
    pdf (PdfPages): The PDF object to save the table.
    """
    fig, ax = plt.subplots(figsize=(12, 4))
    ax.axis('tight')
    ax.axis('off')
    table = ax.table(cellText=metrics_df.values, colLabels=metrics_df.columns, cellLoc='center', loc='center')
    table.auto_set_font_size(False)
    table.set_fontsize(10)
    table.auto_set_column_width(col=list(range(len(metrics_df.columns))))
    pdf.savefig(fig, bbox_inches='tight')
    plt.close()

def generate_report(output_path, head_hands_data, teleport_data, valid_sections):
    """
    Generates a comprehensive report with plots and metrics saved to a PDF.

    Parameters:
    output_path (str): The file path to save the final PDF report.
    head_hands_data (pandas.DataFrame): The dataframe of the head and hands data CSV.
    teleport_data (pandas.DataFrame): The dataframe path of the teleport data CSV.
    valid_sections (list): List of valid sections conceived in your Unity scene.
    """

    head_hands_data = head_hands_data.applymap(lambda x: x.strip() if isinstance(x, str) else x)
    teleport_data = teleport_data.applymap(lambda x: x.strip() if isinstance(x, str) else x)
    head_hands_data.columns = head_hands_data.columns.str.strip()
    teleport_data.columns = teleport_data.columns.str.strip()

    with PdfPages(output_path) as pdf:
        plot_user_presence_with_sections(head_hands_data.copy(), teleport_data.copy(), valid_sections, pdf)
        plot_bubble_plot(head_hands_data.copy(), teleport_data.copy(), valid_sections, pdf)

        teleport_durations = []
        current_duration = 0

        for _, row in teleport_data.iterrows():
            current_duration += row['Duration']
            if row['WasTP']:
                teleport_durations.append(current_duration)
                current_duration = 0

        average_successful_teleport_duration = sum(teleport_durations) / len(teleport_durations) if teleport_durations else 0

        head_hands_data['Time_Delta'] = head_hands_data['Timestamp'].diff().fillna(0)
        time_in_zones = head_hands_data.groupby('Zone')['Time_Delta'].sum()
        stop_move_counts = head_hands_data['Status'].value_counts()
        stop_move_percentages = head_hands_data['Status'].value_counts(normalize=True) * 100

        # METRIC: Calculate the velocity magnitude for each hand
        head_hands_data['Velocity_HandR_Magnitude'] = np.sqrt(
            head_hands_data['Velocity_HandR_x']**2 + 
            head_hands_data['Velocity_HandR_y']**2 + 
            head_hands_data['Velocity_HandR_z']**2
        )

        head_hands_data['Velocity_HandL_Magnitude'] = np.sqrt(
            head_hands_data['Velocity_HandL_x']**2 + 
            head_hands_data['Velocity_HandL_y']**2 + 
            head_hands_data['Velocity_HandL_z']**2
        )

        head_hands_data_clean = remove_outliers(head_hands_data, 'Velocity_HandR_Magnitude')
        head_hands_data_clean = remove_outliers(head_hands_data_clean, 'Velocity_HandL_Magnitude')

        # METRIC: Calculate the number of visits per zone
        head_hands_data_clean['Zone_Shifted'] = head_hands_data_clean['Zone'].shift(1)
        head_hands_data_clean['New_Visit'] = head_hands_data_clean['Zone'] != head_hands_data_clean['Zone_Shifted']
        visit_counts = head_hands_data_clean[head_hands_data_clean['New_Visit']].groupby('Zone').size()

        # Prepare visits data for metrics_df
        visit_counts_str = ", ".join([f"{zone}: {count}" for zone, count in visit_counts.items()])

        # METRIC: Calculate the acceleration for each hand
        average_velocity_magnitude_handR = head_hands_data_clean['Velocity_HandR_Magnitude'].mean()
        std_velocity_magnitude_handR = head_hands_data_clean['Velocity_HandR_Magnitude'].std()

        average_velocity_magnitude_handL = head_hands_data_clean['Velocity_HandL_Magnitude'].mean()
        std_velocity_magnitude_handL = head_hands_data_clean['Velocity_HandL_Magnitude'].std()

        head_hands_data_clean['Acceleration_HandR'] = head_hands_data_clean['Velocity_HandR_Magnitude'].diff().fillna(0) / head_hands_data_clean['Time_Delta']
        head_hands_data_clean['Acceleration_HandL'] = head_hands_data_clean['Velocity_HandL_Magnitude'].diff().fillna(0) / head_hands_data_clean['Time_Delta']

        mean_acceleration_handR = head_hands_data_clean['Acceleration_HandR'].mean()
        std_acceleration_handR = head_hands_data_clean['Acceleration_HandR'].std()

        mean_acceleration_handL = head_hands_data_clean['Acceleration_HandL'].mean()
        std_acceleration_handL = head_hands_data_clean['Acceleration_HandL'].std()

        plt.figure(figsize=(14, 7))
        plt.plot(head_hands_data_clean['Timestamp'], head_hands_data_clean['Velocity_HandR_Magnitude'], label='Right Hand Velocity', color='blue')
        plt.plot(head_hands_data_clean['Timestamp'], head_hands_data_clean['Velocity_HandL_Magnitude'], label='Left Hand Velocity', color='green')
        plt.xlabel('Timestamp')
        plt.ylabel('Velocity Magnitude (m/s)')
        plt.title('Hand Velocity Magnitude Over Time')
        plt.legend()
        pdf.savefig()
        plt.close()

        # METRIC: Calculate the number of stops and moves and percentages
        stop_counts, move_counts = count_stops_and_moves(head_hands_data)
        stop_percentage = (stop_counts) / (stop_counts + move_counts) * 100
        move_percentage = (move_counts) / (stop_counts + move_counts) * 100

        metrics = {
        'Metric': [
            'Stop Count', 'Move Count', 'Stop Percentage', 'Move Percentage', 
            'Average Duration for Successful Teleport', 'Average Velocity HandR_x', 
            'Average Velocity HandR_y', 'Average Velocity HandR_z', 
            'Average Velocity HandL_x', 'Average Velocity HandL_y', 
            'Average Velocity HandL_z', 'Average Velocity Magnitude HandR', 
            'Average Velocity Magnitude HandL', 'Mean Acceleration HandR', 
            'Mean Acceleration HandL', 'Number of Visits per Zone'
        ],
        'Value': [
            stop_counts, move_counts, stop_percentage, move_percentage, average_successful_teleport_duration, 
            head_hands_data_clean['Velocity_HandR_x'].mean(), head_hands_data_clean['Velocity_HandR_y'].mean(), 
            head_hands_data_clean['Velocity_HandR_z'].mean(), head_hands_data_clean['Velocity_HandL_x'].mean(), 
            head_hands_data_clean['Velocity_HandL_y'].mean(), head_hands_data_clean['Velocity_HandL_z'].mean(),
            f"{average_velocity_magnitude_handR:.6f} ± {std_velocity_magnitude_handR:.6f}",
            f"{average_velocity_magnitude_handL:.6f} ± {std_velocity_magnitude_handL:.6f}",
            f"{mean_acceleration_handR:.6f} ± {std_acceleration_handR:.6f}",
            f"{mean_acceleration_handL:.6f} ± {std_acceleration_handL:.6f}",
            visit_counts_str
        ]
    }

        teleport_frames = teleport_data[teleport_data['WasTP'] == True]['Frame']

        head_hands_data['Is_Teleport'] = head_hands_data['Frame'].isin(teleport_frames)

        head_hands_data['Distance_Traveled'] = np.sqrt(
            (head_hands_data['HMD_x'].diff().fillna(0))**2 +
            (head_hands_data['HMD_z'].diff().fillna(0))**2
        )

        # METRIC: Distance traveled without considering teleportations
        total_distance_traveled = 0
        current_distance = 0

        # Iterate over the dataframe and sum distances until teleportation is detected
        for idx, row in head_hands_data.iterrows():
            if row['Is_Teleport']:
                total_distance_traveled += current_distance
                current_distance = 0
            else:
                current_distance += row['Distance_Traveled']

        total_distance_traveled += current_distance

        metrics['Metric'].append('Total Distance Traveled (without teleports)')
        metrics['Value'].append(total_distance_traveled)
        
        mean_std_velocity_by_zone = calculate_mean_velocity(head_hands_data)

        # METRIC: Add mean and standard deviation of velocity by zone to metrics_df
        for _, row in mean_std_velocity_by_zone.iterrows():
            metrics['Metric'].append(f'Mean Velocity in Zone {row["Zone"]}')
            metrics['Value'].append(f'{row["Mean_Velocity"]:.6f} ± {row["Std_Velocity"]:.6f}')

        metrics_df = pd.DataFrame(metrics)
        
        fig, ax = plt.subplots(1, 2, figsize=(14, 6))

        # GRAPH: Bar chart for stop and move counts
        counts_df = pd.DataFrame({
            'Type': ['Stop', 'Move'],
            'Count': [stop_counts, move_counts]
        })
        counts_df.plot(kind='bar', x='Type', y='Count', ax=ax[0], color=['skyblue', 'salmon'], legend=False)
        ax[0].set_title('Stop and Move Counts')
        ax[0].set_xlabel('Type')
        ax[0].set_ylabel('Count')
        for i in ax[0].patches:
            ax[0].text(i.get_x() + i.get_width() / 2, i.get_height() + 2, f'{i.get_height():.0f}', ha='center', va='bottom')

        # GRAPH: Pie chart for stop and move percentages
        percentages_df = pd.DataFrame({
            'Type': ['Stop', 'Move'],
            'Percentage': [stop_percentage, move_percentage]
        })
        percentages_df.set_index('Type').plot(kind='pie', y='Percentage', autopct='%1.1f%%', ax=ax[1], colors=['skyblue', 'salmon'])
        ax[1].set_title('Stop and Move Percentages')
        ax[1].set_ylabel('')

        pdf.savefig(fig)
        plt.close()

        fig, ax = plt.subplots(1, 2, figsize=(14, 6))
        time_in_zones.plot(kind='bar', ax=ax[0], color='lightgreen')
        ax[0].set_title('Total Time Spent in Each Zone')
        ax[0].set_xlabel('Zone')
        ax[0].set_ylabel('Total Time (seconds)')
        ax[0].tick_params(axis='x', rotation=45)

        for i in ax[0].patches:
            ax[0].text(i.get_x() + i.get_width() / 2, i.get_height() + 2, f'{i.get_height():.0f}', ha='center', va='bottom')

        average_time_per_visit = time_in_zones / visit_counts
        average_time_per_visit.plot(kind='pie', autopct=lambda p: f'{p:.1f}% ({p * sum(average_time_per_visit) / 100:.2f}s)', ax=ax[1], colors=plt.cm.Paired(range(len(average_time_per_visit))))
        ax[1].set_title('Average Time Spent in Each Zone Per Visit')
        ax[1].set_ylabel('')

        pdf.savefig(fig)
        plt.close()

        teleport_hotspot_counts = teleport_data['TPHotspot'].value_counts()
        fig, ax = plt.subplots(figsize=(10, 6))
        teleport_hotspot_counts.plot(kind='bar', ax=ax, color='orchid')
        ax.set_title('Visits to Each Teleport Hotspot')
        ax.set_xlabel('Teleport Hotspot')
        ax.set_ylabel('Count')
        ax.tick_params(axis='x', rotation=45)

        pdf.savefig(fig)
        plt.close()

        save_metrics_table_to_pdf(metrics_df, pdf)