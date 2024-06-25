import os
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from matplotlib.backends.backend_pdf import PdfPages
from reportlab.lib.pagesizes import letter
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib import colors
import PyPDF2

def ensure_directory_exists(directory):
    """
    Ensures that the specified directory exists. If it doesn't exist, creates it.

    Parameters:
    directory (str): The path of the directory to check or create.
    """
    if not os.path.exists(directory):
        os.makedirs(directory)

def generate_graphs(df, graphics_pdf_path):
    """
    Generates various graphs based on the provided DataFrame and saves them to a PDF.

    Parameters:
    df (pandas.DataFrame): The DataFrame containing the data to plot.
    graphics_pdf_path (str): The file path to save the generated PDF.
    """

    # Calculate time spent in each frame
    df['Time_Delta'] = df['Timestamp'].diff().fillna(0)

    # Identify continuous blocks (visits) for AOI
    df['AOI_Shifted'] = df['Product/AOI'].shift(1)
    df['New_AOI_Visit'] = df['Product/AOI'] != df['AOI_Shifted']
    df['AOI_Visit_ID'] = df['New_AOI_Visit'].cumsum()

    # Identify continuous blocks (visits) for Shelf
    df['Shelf_Shifted'] = df['Section/Shelf'].shift(1)
    df['New_Shelf_Visit'] = df['Section/Shelf'] != df['Shelf_Shifted']
    df['Shelf_Visit_ID'] = df['New_Shelf_Visit'].cumsum()

    # Identify continuous blocks (visits) for AOI by Shelf
    df['AOI_Shelf_Shifted'] = df['Product/AOI'] + df['Section/Shelf']
    df['New_AOI_Shelf_Visit'] = df['AOI_Shelf_Shifted'].shift(1) != df['AOI_Shelf_Shifted']
    df['AOI_Shelf_Visit_ID'] = df['New_AOI_Shelf_Visit'].cumsum()

    # Calculate the number of visits per AOI
    aoi_counts = df.groupby('AOI_Visit_ID')['Product/AOI'].first().value_counts()

    # Calculate the number of visits per Section/Shelf
    section_counts = df.groupby('Shelf_Visit_ID')['Section/Shelf'].first().value_counts()

    # Calculate the number of visits per Section/Shelf for each AOI
    aoi_section_counts = df.groupby(['Section/Shelf', 'Product/AOI'])['AOI_Shelf_Visit_ID'].nunique().unstack().fillna(0)

    # Calculate the total time spent per visit in each AOI
    aoi_visit_time = df.groupby('AOI_Visit_ID')['Time_Delta'].sum().reset_index()
    aoi_visit_time = aoi_visit_time.merge(df[['AOI_Visit_ID', 'Product/AOI']].drop_duplicates(), on='AOI_Visit_ID')
    aoi_visit_time = aoi_visit_time.groupby('Product/AOI')['Time_Delta'].sum()

    # Calculate the total time spent per visit in each Shelf
    shelf_visit_time = df.groupby('Shelf_Visit_ID')['Time_Delta'].sum().reset_index()
    shelf_visit_time = shelf_visit_time.merge(df[['Shelf_Visit_ID', 'Section/Shelf']].drop_duplicates(), on='Shelf_Visit_ID')
    shelf_visit_time = shelf_visit_time.groupby('Section/Shelf')['Time_Delta'].sum()

    # Calculate the total time spent per visit in each AOI by Shelf
    aoi_shelf_visit_time = df.groupby('AOI_Shelf_Visit_ID')['Time_Delta'].sum().reset_index()
    aoi_shelf_visit_time = aoi_shelf_visit_time.merge(df[['AOI_Shelf_Visit_ID', 'Section/Shelf', 'Product/AOI']].drop_duplicates(), on='AOI_Shelf_Visit_ID')
    aoi_shelf_visit_time = aoi_shelf_visit_time.groupby(['Section/Shelf', 'Product/AOI'])['Time_Delta'].sum().unstack().fillna(0)

    # Additional statistics for new requests
    df.loc[:, 'Time_Diff'] = df['Timestamp'].diff().fillna(0)
    total_observation_time = df.groupby('Product/AOI')['Time_Diff'].sum()
    total_observation_time_by_section = df.groupby(['Section/Shelf', 'Product/AOI'])['Time_Diff'].sum().unstack(fill_value=0)
    
    # Mean velocity in AOI
    df.loc[:, 'Velocity'] = ((df['RCHit_x'].diff()**2 + df['RCHit_y'].diff()**2 + df['RCHit_z'].diff()**2)**0.5) / df['Timestamp'].diff()
    mean_velocity_aoi = df.groupby(['Section/Shelf', 'Product/AOI'])['Velocity'].mean().unstack(fill_value=0).fillna(0).round(2)

    with PdfPages(graphics_pdf_path) as pdf:

        # Number of visits per AOI
        plt.figure(figsize=(10, 6))
        ax = sns.barplot(x=aoi_counts.index, y=aoi_counts.values)
        plt.title('Number of Visits per AOI')
        plt.xlabel('AOI')
        plt.ylabel('Number of Visits')
        plt.xticks(rotation=45, ha='right')
        for container in ax.containers:
            ax.bar_label(container)
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Number of visits per Section/Shelf
        plt.figure(figsize=(10, 6))
        ax = sns.barplot(x=section_counts.index, y=section_counts.values)
        plt.title('Number of Visits per Section/Shelf')
        plt.xlabel('Section/Shelf')
        plt.ylabel('Number of Visits')
        plt.xticks(rotation=90, ha='right')
        for container in ax.containers:
            ax.bar_label(container)
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Number of visits per Section/Shelf for each AOI
        fig, ax = plt.subplots(figsize=(12, 8))
        aoi_section_counts.plot(kind='bar', stacked=True, ax=ax)
        plt.title('Number of Visits per Section/Shelf for each AOI')
        plt.xlabel('Section/Shelf')
        plt.ylabel('Number of Visits')
        plt.xticks(rotation=90, ha='right')
        for container in ax.containers:
            ax.bar_label(container)
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # AOI visit time
        plt.figure(figsize=(10, 6))
        sns.barplot(x=aoi_visit_time.index, y=aoi_visit_time.values)
        plt.title('Total Time Spent per AOI')
        plt.xlabel('AOI')
        plt.ylabel('Total Time (seconds)')
        plt.xticks(rotation=45, ha='right')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Plot the total time spent in each Section/Shelf
        plt.figure(figsize=(10, 6))
        sns.barplot(x=shelf_visit_time.index, y=shelf_visit_time.values)
        plt.title('Total Time Spent per Section/Shelf')
        plt.xlabel('Section/Shelf')
        plt.ylabel('Total Time (seconds)')
        plt.xticks(rotation=90, ha='right')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Plot the total time spent per Section/Shelf for each AOI
        plt.figure(figsize=(12, 8))
        aoi_shelf_visit_time.plot(kind='bar', stacked=True, figsize=(12, 8))
        plt.title('Total Time Spent per Section/Shelf for each AOI')
        plt.xlabel('Section/Shelf')
        plt.ylabel('Total Time (seconds)')
        plt.xticks(rotation=90, ha='right')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Mean velocity per AOI by Section/Shelf
        plt.figure(figsize=(12, 8))
        mean_velocity_aoi.plot(kind='bar', stacked=True, figsize=(12, 8))
        plt.title('Mean Velocity per AOI by Section/Shelf')
        plt.xlabel('Section/Shelf')
        plt.ylabel('Mean Velocity')
        plt.xticks(rotation=90, ha='right')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

        # Temporal graph of fixations over time
        plt.figure(figsize=(10, 6))
        plt.plot(df['Timestamp'], df['fixation'], linestyle='None', marker='o', markersize=2)
        plt.title('Temporal Graph of Fixations')
        plt.xlabel('Timestamp')
        plt.ylabel('Fixation (1=True, 0=False)')
        plt.tight_layout()
        pdf.savefig()
        plt.close()

def create_statistics_pdf(df, aoi_counts, aoi_section_counts, aoi_percentages, section_counts, total_observation_time, total_observation_time_by_section, total_observation_time_by_section_agg, mean_visit_time, mean_velocity_aoi, 
                          mean_velocity_type, fixation_counts, saccade_counts, total_fixation_time, total_saccade_time, fixation_percentage, 
                          saccade_percentage, mean_fixation_duration, statistics_pdf_path):
    """
    Creates a PDF document containing various statistics tables.

    Parameters:
    df (pandas.DataFrame): The DataFrame containing the data.
    aoi_counts, aoi_section_counts, aoi_percentages, section_counts, total_observation_time, total_observation_time_by_section, 
    total_observation_time_by_section_agg, mean_visit_time, mean_velocity_aoi, mean_velocity_type, fixation_counts, 
    saccade_counts, total_fixation_time, total_saccade_time, fixation_percentage, saccade_percentage, mean_fixation_duration: Statistics data obtained from collected VR data.
    statistics_pdf_path (str): The file path to save the generated PDF.
    """
    
    ensure_directory_exists(os.path.dirname(statistics_pdf_path))
    
    doc = SimpleDocTemplate(statistics_pdf_path, pagesize=letter)
    elements = []

    styles = getSampleStyleSheet()
    title = Paragraph("AOIs statistics", styles['Title'])
    elements.append(title)
    elements.append(Spacer(1, 12))

    total_observation_time = total_observation_time.fillna(0).round(2)
    total_observation_time_by_section = total_observation_time_by_section.fillna(0).round(2)
    section_counts = section_counts.fillna(0).round(2)
    aoi_counts = aoi_counts.fillna(0).round(2)
    aoi_percentages = aoi_percentages.fillna(0).round(2)

    # AOI statistics table
    aoi_data = [["AOI", "Count", "Percentage", "Total Observation Time (s)"]] + list(pd.concat([aoi_counts, aoi_percentages, total_observation_time], axis=1).reset_index().values)
    aoi_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in aoi_data]
    aoi_table = Table(aoi_data)
    aoi_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(aoi_table)
    elements.append(Spacer(1, 24))

    # Section/Shelf statistics table
    aoi_section_counts_new = df.groupby(['Section/Shelf', 'Product/AOI']).size().unstack(fill_value=0).stack().reset_index(name='Count')
    total_observation_time_by_section = df.groupby(['Section/Shelf', 'Product/AOI'])['Time_Diff'].sum().unstack(fill_value=0).stack().reset_index(name='Total Observation Time (s)')
    combined_data = pd.merge(aoi_section_counts_new, total_observation_time_by_section, on=['Section/Shelf', 'Product/AOI'], how='outer').fillna(0)

    section_data = [["Section/Shelf", "Product/AOI", "Count", "Total Observation Time (s)"]] + list(combined_data.values)
    section_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in section_data]
    section_table = Table(section_data)
    section_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(section_table)
    elements.append(Spacer(1, 24))

    # Mean visit time statistics table
    mean_visit_time = mean_visit_time.fillna(0).round(2)
    mean_visit_data = [["Section/Shelf", "AOI", "Mean Visit Time (s)"]] + list(mean_visit_time.reset_index().values)
    mean_visit_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in mean_visit_data]
    mean_visit_table = Table(mean_visit_data)
    mean_visit_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(mean_visit_table)
    elements.append(Spacer(1, 24))

    # Mean velocity statistics table
    mean_velocity_data = [["Section/Shelf", "AOI", "Mean Velocity"]] + list(mean_velocity_aoi.stack().reset_index().values)
    mean_velocity_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in mean_velocity_data]
    mean_velocity_table = Table(mean_velocity_data)
    mean_velocity_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(mean_velocity_table)
    elements.append(Spacer(1, 24))

    # Fixation and saccade statistics table
    fixation_saccade_data = [
        ["Metric", "Value"],
        ["Total Fixation Time (s)", round(total_fixation_time, 2)],
        ["Total Saccade Time (s)", round(total_saccade_time, 2)],
        ["Fixation Percentage (%)", round(fixation_percentage, 2)],
        ["Saccade Percentage (%)", round(saccade_percentage, 2)],
        ["Mean Fixation Duration (s)", round(mean_fixation_duration.mean() if not mean_fixation_duration.empty else 0, 2)]
    ]
    fixation_saccade_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in fixation_saccade_data]
    fixation_saccade_table = Table(fixation_saccade_data)
    fixation_saccade_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(fixation_saccade_table)
    elements.append(Spacer(1, 24))

    fixations = []
    current_fixation = None

    for _, row in df.iterrows():
        if row['fixation_start'] == 1:
            current_fixation = row
        elif row['fixation_end'] == 1 and current_fixation is not None:
            fixation_duration = row['Timestamp'] - current_fixation['Timestamp']
            fixations.append({
                'Section/Shelf': current_fixation['Section/Shelf'],
                'Product/AOI': current_fixation['Product/AOI'],
                'fixation_duration': fixation_duration
            })
            current_fixation = None

    fixations_df = pd.DataFrame(fixations)

    # Calcular el número de fijaciones y la duración total
    total_fixation_duration = fixations_df.groupby(['Section/Shelf', 'Product/AOI'])['fixation_duration'].sum().reset_index(name='Total Fixation Duration').round(2)
    fixation_times = fixations_df.groupby(['Section/Shelf', 'Product/AOI']).size().reset_index(name='Fixation Times')
    fixation_data = pd.merge(total_fixation_duration, fixation_times, on=['Section/Shelf', 'Product/AOI'], how='outer').fillna(0)

    fixation_table_data = [["Section/Shelf", "AOI", "Total Fixation Duration (s)", "Fixation Times"]] + list(fixation_data.values)
    fixation_table_data = [[Paragraph(str(cell), styles['Normal']) for cell in row] for row in fixation_table_data]
    fixation_table = Table(fixation_table_data)
    fixation_table.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), colors.grey),
        ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
        ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
        ('BACKGROUND', (0, 1), (-1, -1), colors.beige),
        ('GRID', (0, 0), (-1, -1), 1, colors.black),
    ]))
    elements.append(fixation_table)
    elements.append(Spacer(1, 24))

    doc.build(elements)

def combine_pdfs(graphics_pdf_path, statistics_pdf_path, final_pdf_path):
    """
    Combines two PDF files into one.

    Parameters:
    graphics_pdf_path (str): The file path of the graphics PDF.
    statistics_pdf_path (str): The file path of the statistics PDF.
    final_pdf_path (str): The file path to save the combined PDF.
    """
    merger = PyPDF2.PdfMerger()

    for pdf in [graphics_pdf_path, statistics_pdf_path]:
        merger.append(pdf)

    merger.write(final_pdf_path)
    merger.close()

def generate_statistics_report_ET(df, final_pdf_path):
    """
    Generates a comprehensive statistics report PDF from the provided DataFrame.

    Parameters:
    df (pandas.DataFrame): The DataFrame containing the data to analyze.
    final_pdf_path (str): The file path to save the final combined PDF.
    """
    graphics_pdf_path = './reports/VR_SI_Graphics.pdf'
    statistics_pdf_path = './reports/VR_SI_Statistics.pdf'

    # Generate graphs and statistics PDFs
    generate_graphs(df, graphics_pdf_path)
    aoi_counts = df['Product/AOI'].value_counts()
    aoi_percentages = df['Product/AOI'].value_counts(normalize=True).round(2) * 100
    section_counts = df['Section/Shelf'].value_counts()
    aoi_section_counts = df.groupby(['Section/Shelf', 'Product/AOI']).size().unstack(fill_value=0).stack().fillna(0).round(2)
    
    df.loc[:, 'Time_Diff'] = df['Timestamp'].diff().fillna(0).round(2)
    total_observation_time = df.groupby('Product/AOI')['Time_Diff'].sum().fillna(0).round(2)
    total_observation_time_by_section = df.groupby(['Section/Shelf', 'Product/AOI'])['Time_Diff'].sum().unstack(fill_value=0).fillna(0).round(2)
    total_observation_time_by_section_agg = df.groupby(['Section/Shelf'])['Time_Diff'].sum().fillna(0).round(2)

    # Mean visit time calculation
    df.loc[:, 'Previous_AOI'] = df['Product/AOI'].shift(1)
    df.loc[:, 'New_Visit'] = df['Product/AOI'] != df['Previous_AOI']
    df.loc[:, 'Visit_ID'] = df['New_Visit'].cumsum()
    visit_times = df.groupby(['Visit_ID', 'Product/AOI', 'Section/Shelf'])['Timestamp'].agg(['first', 'last'])
    visit_times['Visit_Duration'] = (visit_times['last'] - visit_times['first']).fillna(0).round(2)
    mean_visit_time = visit_times.groupby(['Section/Shelf', 'Product/AOI'])['Visit_Duration'].mean().fillna(0).round(2)

    # Mean velocity calculation
    df.loc[:, 'Velocity'] = ((df['RCHit_x'].diff()**2 + df['RCHit_y'].diff()**2 + df['RCHit_z'].diff()**2)**0.5) / df['Timestamp'].diff()
    mean_velocity_aoi = df.groupby(['Section/Shelf', 'Product/AOI'])['Velocity'].mean().unstack(fill_value=0).fillna(0).round(2)
    mean_velocity_type = df.groupby('Product/AOI')['Velocity'].mean().fillna(0).round(2)

    # Fixation and saccade statistics
    fixation_counts = df[df['fixation'] == 1].groupby(['Section/Shelf', 'Product/AOI']).size().unstack(fill_value=0).fillna(0)
    saccade_counts = df[df['fixation'] == 0].groupby(['Section/Shelf', 'Product/AOI']).size().unstack(fill_value=0).fillna(0)
    total_fixation_time = df[df['fixation'] == 1]['Time_Diff'].sum()
    total_saccade_time = df[df['fixation'] == 0]['Time_Diff'].sum()
    total_time = total_fixation_time + total_saccade_time
    fixation_percentage = (total_fixation_time / total_time) * 100
    saccade_percentage = (total_saccade_time / total_time) * 100
    mean_fixation_duration = df[df['fixation_duration'] != 0].groupby(['Section/Shelf', 'Product/AOI'])['fixation_duration'].mean().unstack(fill_value=0).fillna(0).round(2)

    # Create statistics PDF
    create_statistics_pdf(df,
        aoi_counts,
        aoi_section_counts,
        aoi_percentages,
        section_counts,
        total_observation_time,
        total_observation_time_by_section,
        total_observation_time_by_section_agg,
        mean_visit_time,
        mean_velocity_aoi,
        mean_velocity_type,
        fixation_counts,
        saccade_counts,
        total_fixation_time,
        total_saccade_time,
        fixation_percentage,
        saccade_percentage,
        mean_fixation_duration,
        statistics_pdf_path
    )

    # Combine PDFs
    combine_pdfs(graphics_pdf_path, statistics_pdf_path, final_pdf_path)

def sanitize_dataframe(df):
    """
    Drops duplicate frames, keeping the first occurrence.

    Parameters:
    df (pandas.DataFrame): The DataFrame to sanitize.

    Returns:
    pandas.DataFrame: The sanitized DataFrame.
    """
    sanitized_df = df.drop_duplicates(subset=['Frame'])
    return sanitized_df

##### IF YOU WANT TO TEST THIS FUNCTIONALITY, UNCOMMENT THE FOLLOWING CODE AND CHANGE THE file_path VALUE TO THE .CSV FILE OF YOUR CONVENIENCE#####
# file_path = './EyeTrackerData-AOIBigEnvironment_withFixations.csv'
# df = pd.read_csv(file_path)
# df_new = sanitize_dataframe(df)
# final_pdf_path = './reports/VR_SI_Statistics_Module_Combined.pdf'
# generate_statistics_report_ET(df_new, final_pdf_path)