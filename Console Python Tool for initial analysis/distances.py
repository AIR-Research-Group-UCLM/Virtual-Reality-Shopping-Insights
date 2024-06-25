import pandas as pd
import numpy as np
from warnings import filterwarnings

def euclidean_distance(point1, point2):
    """
    Calculates the Euclidean distance between two points in 3D space.

    Parameters:
    point1 (list-like): Coordinates of the first point (x, y, z).
    point2 (list-like): Coordinates of the second point (x, y, z).

    Returns:
    float: Euclidean distance between the two points.
    """
    return np.sqrt((point1[0] - point2[0])**2 + (point1[1] - point2[1])**2 + (point1[2] - point2[2])**2)

def compute_movement(df, threshold=None):
    """
    Computes movement status based on the Euclidean distance between consecutive points.

    Parameters:
    df (pandas.DataFrame): DataFrame containing 'HMD_x', 'HMD_y', 'HMD_z' columns for 3D coordinates.
    threshold (float, optional): Threshold distance to determine if movement occurred. Defaults to 0.01.

    Returns:
    pandas.DataFrame: DataFrame with an additional 'Status' column indicating 'Stop' or 'Move'.
    """
    distances = []

    filterwarnings("ignore")

    for i in range(1, len(df)):
        current_point = df.loc[i, ['HMD_x', 'HMD_y', 'HMD_z']]
        previous_point = df.loc[i-1, ['HMD_x', 'HMD_y', 'HMD_z']]
        distance = euclidean_distance(current_point, previous_point)
        distances.append(distance)

    if threshold is None:
        threshold = 0.01

    states = []

    # Calculate the Euclidean distance between each row and the previous one and determine the state
    for i in range(1, len(df)):
        current_point = df.loc[i, ['HMD_x', 'HMD_y', 'HMD_z']]
        previous_point = df.loc[i-1, ['HMD_x', 'HMD_y', 'HMD_z']]
        distance = euclidean_distance(current_point, previous_point)
        state = "Stop" if distance < threshold else "Move"
        states.append(state)

    df['Status'] = ['Stop'] + states  # Assume the first row is "Stop"

    return df
