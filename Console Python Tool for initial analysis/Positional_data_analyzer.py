import pandas as pd
import matplotlib.pyplot as plt

# Load the provided CSV file
file_path = './sample_data/ShelvesDataBigEnvironment.csv'
shelves_data = pd.read_csv(file_path)

shelves_data = shelves_data.applymap(lambda x: x.strip() if isinstance(x, str) else x)
shelves_data.columns = shelves_data.columns.str.strip()

# Calculate total time spent in each Shelf and AOI
shelves_data['Time_Delta'] = shelves_data['Timestamp'].diff().fillna(0)

# Group by Shelf and AOI to sum the time spent
time_per_shelf_aoi = shelves_data.groupby(['Shelf', 'AOI'])['Time_Delta'].sum().unstack().fillna(0)

# Plot the data
ax = time_per_shelf_aoi.plot(kind='bar', stacked=True, figsize=(14, 7))
ax.set_title('Total Time Spent in Each Shelf and AOI with the Hands in the Shelves')
ax.set_xlabel('Shelf')
ax.set_ylabel('Total Time (seconds)')
ax.legend(title='AOI')

plt.tight_layout()

plt.savefig('./reports/total_time_spent_per_shelf_and_aoi.png')