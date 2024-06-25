import pandas as pd
import seaborn as sns
import numpy as np
import matplotlib.pyplot as plt
from reportlab.lib.pagesizes import letter
from reportlab.pdfgen import canvas
from reportlab.lib.utils import ImageReader
from matplotlib.backends.backend_pdf import PdfPages

def count_interactions(data):
    """
    Counts the interactions per product based on the data provided. An interaction is counted as the time a product is started to be interacted with until it is released.
    For interactions other than grab, the interaction starts when its interactable state is 'Select' until that states changes.

    Parameters:
    data (pandas.DataFrame): DataFrame containing the interaction data with 'Object' and 'State' columns.

    Returns:
    dict: A dictionary with products as keys and interaction counts as values.
    """
    interactions = {}
    current_object = None
    state = None

    for index, row in data.iterrows():
        obj = row['Object']
        state = row['State'].strip()

        if obj != current_object and (state != 'Select'):
            if current_object is not None:
                if current_object in interactions:
                    interactions[current_object] += 1
                else:
                    interactions[current_object] = 1
            current_object = obj
        else:
            if state != 'Select':
                if current_object in interactions:
                    interactions[current_object] += 1
                else:
                    interactions[current_object] = 1

    # Add the last interaction
    if current_object is not None:
        if current_object in interactions:
            interactions[current_object] += 1
        else:
            interactions[current_object] = 1

    return interactions

def count_interactions_by_hand(data):
    """
    Counts the interactions by hand (Right Hand, Left Hand, Both Hands) based on the data provided.

    Parameters:
    data (pandas.DataFrame): DataFrame containing the interaction data with 'Object', 'RightHand_State', and 'LeftHand_State' columns.

    Returns:
    dict: A dictionary with hand types as keys and interaction counts as values.
    """
    interactions = {'Right Hand': 0, 'Left Hand': 0, 'Both Hands': 0}
    current_object = None
    current_rh_state = None
    current_lh_state = None

    for index, row in data.iterrows():
        obj = row['Object']
        rh_state = row['RightHand_State'].strip()
        lh_state = row['LeftHand_State'].strip()
        
        if obj != current_object or rh_state != current_rh_state or lh_state != current_lh_state:
            if current_object is not None:
                if current_rh_state == 'Select' and current_lh_state == 'Select':
                    interactions['Both Hands'] += 1
                elif current_rh_state == 'Select':
                    interactions['Right Hand'] += 1
                elif current_lh_state == 'Select':
                    interactions['Left Hand'] += 1
            current_object = obj
            current_rh_state = rh_state
            current_lh_state = lh_state

    # Add the last interaction
    if current_object is not None:
        if current_rh_state == 'Select' and current_lh_state == 'Select':
            interactions['Both Hands'] += 1
        elif current_rh_state == 'Select':
            interactions['Right Hand'] += 1
        elif current_lh_state == 'Select':
            interactions['Left Hand'] += 1

    return interactions

def calculate_average_durations(data):
    """
    Calculates the average duration of interactions for each product.

    Parameters:
    data (pandas.DataFrame): DataFrame containing the interaction data with 'Object' and 'DurationUntilRelease' columns.

    Returns:
    pandas.DataFrame: DataFrame with products and their average interaction durations.
    """
    data = data[~data['Object'].isin(['HandGrabInteractable', 'HandGrabInteractable_mirror'])]
    average_durations = data.groupby('Object')['DurationUntilRelease'].mean()
    average_durations_df = average_durations.reset_index()
    average_durations_df.columns = ['Product', 'AverageDuration']
    return average_durations_df

def generate_pdf_report(productInteractionDataFrame=None, shoppingCartDataFrame=None, productReleasesDataFrame=None):
    """
    Generates a PDF report with various interaction statistics and graphs.

    Parameters:
    productInteractionDataFrame (pandas.DataFrame, optional): DataFrame with product interaction data.
    shoppingCartDataFrame (pandas.DataFrame, optional): DataFrame with shopping cart data.
    productReleasesDataFrame (pandas.DataFrame, optional): DataFrame with product release data.
    """
    
    # --- GRAPHS ---
    if productInteractionDataFrame is None:
        productInteractionDataFrame = pd.read_csv('./sample_data/ProductInteractionDataBigEnvironment.csv') 
    if shoppingCartDataFrame is None:
        shoppingCartDataFrame = pd.read_csv('./sample_data/ShoppingCartDataBigEnvironment.csv')
    if productReleasesDataFrame is None:
        productReleasesDataFrame = pd.read_csv('./sample_data/ProductReleasesBigEnvironment.csv')

    # Sanytize data
    shoppingCartDataFrame = shoppingCartDataFrame.applymap(lambda x: x.strip() if isinstance(x, str) else x)
    productInteractionDataFrame = productInteractionDataFrame.applymap(lambda x: x.strip() if isinstance(x, str) else x)
    productReleasesDataFrame = productReleasesDataFrame.applymap(lambda x: x.strip() if isinstance(x, str) else x)
    productInteractionDataFrame.columns = productInteractionDataFrame.columns.str.strip()
    shoppingCartDataFrame.columns = shoppingCartDataFrame.columns.str.strip()
    productReleasesDataFrame.columns = productReleasesDataFrame.columns.str.strip()

    average_durations_df = calculate_average_durations(productReleasesDataFrame)

    interactions = count_interactions(productInteractionDataFrame)
    interactions_df = pd.DataFrame(list(interactions.items()), columns=['Product', 'InteractionCount'])

    hand_interactions = count_interactions_by_hand(productInteractionDataFrame)
    labels = list(hand_interactions.keys())
    sizes = list(hand_interactions.values())
    colors = ['skyblue', 'lightgreen', 'lightcoral']

    section_counts = (productInteractionDataFrame['Section'].value_counts(normalize=True) * 100).round(2)
    sections = section_counts.index
    percentages = section_counts.values
    angles = np.linspace(0, 2 * np.pi, len(sections), endpoint=False).tolist()
    angles += angles[:1]
    percentages = np.append(percentages, percentages[0])

    grouped = shoppingCartDataFrame.groupby(['Item', 'Action']).size().unstack(fill_value=0)
    objects = [obj for obj in interactions.keys() if obj in grouped.index and (grouped.loc[obj, 'ADD'] != 0 or grouped.loc[obj, 'REMOVE'] != 0)]
    interaction_values = [interactions[obj] for obj in objects]
    add_values = [grouped.loc[obj, 'ADD'] if obj in grouped.index else 0 for obj in objects]
    remove_values = [grouped.loc[obj, 'REMOVE'] if obj in grouped.index else 0 for obj in objects]
    x = range(len(objects))
    x = np.arange(len(objects))
    width = 0.3

    # Compute conversion rate
    total_interactions = sum(interactions.values())
    total_additions = shoppingCartDataFrame[shoppingCartDataFrame['Action'] == 'ADD'].shape[0]
    conversion_ratio = total_additions / total_interactions

    add_actions = shoppingCartDataFrame[shoppingCartDataFrame['Action'] == 'ADD']
    remove_actions = shoppingCartDataFrame[shoppingCartDataFrame['Action'] == 'REMOVE']
    time_differences = {}

    for index, add_row in add_actions.iterrows():
        product = add_row['Item']
        add_time = add_row['Timestamp']
        remove_row = remove_actions[(remove_actions['Item'] == product) & (remove_actions['Timestamp'] > add_time)].head(1)

        if not remove_row.empty:
            remove_time = remove_row['Timestamp'].values[0]
            time_diff = remove_time - add_time

            if product in time_differences:
                time_differences[product].append(time_diff)
            else:
                time_differences[product] = [time_diff]

    average_time_differences = {product: sum(times) / len(times) for product, times in time_differences.items()}
    
    with PdfPages('./reports/VRSI_ProductInteraction_report.pdf') as pdf:

        # 1) Number of interactions per product
        plt.figure(figsize=(12, 8))
        plt.barh(interactions_df['Product'], interactions_df['InteractionCount'], color='skyblue')
        plt.xlabel('Number of Interactions')
        plt.title('Number of interactions per product')
        pdf.savefig()
        plt.close()

        # 2) Average interaction per product
        plt.figure(figsize=(12, 8))
        plt.barh(average_durations_df['Product'].values, average_durations_df['AverageDuration'].values, color='lightgreen')
        for index, value in enumerate(average_durations_df['AverageDuration'].values):
            plt.text(value, index, f"{value:.2f}", va='center')
        plt.xlabel('Average Interaction Duration (seconds)')
        plt.title('Average interaction per product')
        pdf.savefig()
        plt.close()

        # 3) Pie Chart for hand states
        plt.figure(figsize=(8, 8))
        plt.pie(sizes, labels=labels, colors=colors, autopct='%1.1f%%', startangle=140)
        plt.title('Percentage of Interactions by Hand')
        pdf.savefig()
        plt.close()

        # --- STATISTICS ---
        fig, ax = plt.subplots(figsize=(10, 10), subplot_kw=dict(polar=True))
        ax.fill(angles, percentages, color='red', alpha=0.25)
        ax.plot(angles, percentages, color='red', linewidth=2)
        ax.set_yticklabels([])
        ax.set_xticks(angles[:-1])
        ax.set_xticklabels(sections, size=15)

        for i in range(len(sections)):
            angle = angles[i]
            percent = percentages[i]
            ax.text(angle + 0.15, percent + 2, f'{percent:.1f}%', horizontalalignment='center', size=12, color='black', weight='semibold')

        plt.title('Percentage of Interactions by Section', size=20, color='black', y=1.1)
        pdf.savefig()
        plt.close()

        # 5) Bar Chart of interactions vs Additions vs Removals
        fig, ax = plt.subplots()
        rects1 = ax.bar(x - width, interaction_values, width, label='Interactions')
        rects2 = ax.bar(x, add_values, width, label='ADD')
        rects3 = ax.bar(x + width, remove_values, width, label='REMOVE')

        ax.set_ylabel('Counts')

        ax.set_title('Counts by interaction, add and remove actions')
        ax.set_xticks(x)
        ax.set_xticklabels(objects)
        ax.legend()

        fig.tight_layout()
        pdf.savefig()
        plt.close()

        data = {
            'Statistic': ['Total Interactions', 'Total additions', 'Conversion rate' ],
            'Value': [total_interactions, total_additions, conversion_ratio ]
        }
        df = pd.DataFrame(data)
        df_average_time_differences = pd.DataFrame(list(average_time_differences.items()), columns=['Product', 'AverageTimeDifference'])
        df_average_time_differences['AverageTimeDifference'] = df_average_time_differences['AverageTimeDifference'].round(2)

        fig = plt.figure(figsize=(9,2))
        ax = plt.subplot(111)
        ax.axis('off')
        c = df.shape[1]
        ax.table(cellText=df.values, colLabels=df.columns, bbox=[0,0,1,1], )
        pdf.savefig()

        fig = plt.figure(figsize=(9,2))
        ax = plt.subplot(111)
        ax.axis('off')
        cell_text = df_average_time_differences.values
        ax.table(cellText=cell_text, colLabels=df_average_time_differences.columns, bbox=[0,0,1,1])
        pdf.savefig()
