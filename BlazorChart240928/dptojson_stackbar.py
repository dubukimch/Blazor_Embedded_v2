import pandas as pd
from sqlalchemy import create_engine
import re
import os
import time
import json

# MariaDB connection settings
engine = create_engine('mysql+pymysql://dubukimch:7241@172.30.1.97:3306/bnk')

# Start measuring time
start_time = time.time()

# Reading data from 'dive_cust_info' table
cust_info = pd.read_sql_query("SELECT * FROM dive_cust_info", engine)

# Reading data from 'dive_apps_log' table
app_log = pd.read_sql_query("SELECT * FROM dive_apps_log", engine)

# Stop measuring time
end_time = time.time()
read_time = end_time - start_time
print(f"Time taken to read data from MariaDB: {read_time:.2f} seconds")

# Check column names
print("cust_info columns:", cust_info.columns)
print("app_log columns:", app_log.columns)

# Adjust column names
cust_info.columns = ['Age', 'Gender', 'Region', 'AssetBalance', 'DebtBalance', 'CardUsage', 'SeqNo']
app_log.columns = ['EventDate', 'EventTime', 'EventName', 'EventType', 'EventID', 'ClickButton', 'SeqNo']

# Function to extract numerical values from text
def extract_value(text):
    try:
        return float(re.findall(r'\d+', text.replace('만', '0000'))[0])
    except IndexError:
        return 0.0

# Data processing
cust_info['AverageAssets'] = cust_info['AssetBalance'].apply(extract_value)
cust_info['AverageDebt'] = cust_info['DebtBalance'].apply(extract_value)
cust_info['CardUsageValue'] = cust_info['CardUsage'].apply(extract_value)

# Merging customer info and app log data (based on SeqNo)
merged_data = pd.merge(cust_info, app_log, on='SeqNo', how='inner')

# 4. Event frequency by age group for a stacked bar chart
age_event_freq_data = merged_data.groupby(['Age', 'EventName']).size().unstack(fill_value=0)  # Pivot the EventName to columns

# Convert to JSON format suitable for a stacked bar chart
age_event_freq = {
    "XAxis": age_event_freq_data.index.tolist(),  # Age groups
    "EventNames": age_event_freq_data.columns.tolist(),  # Event names (each stack)
    "Data": []  # List of Y-axis values for each event
}

# Add data for each event to the 'Data' list
for event_name in age_event_freq_data.columns:
    age_event_freq['Data'].append({
        "label": event_name,
        "data": age_event_freq_data[event_name].tolist()
    })

# Path to save the JSON files
output_dir = "/home/dubukimch/blazor/wwwroot/data"
os.makedirs(output_dir, exist_ok=True)

# Function to save data as JSON
def save_json(data, filename):
    with open(os.path.join(output_dir, filename), 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

# Save the event frequency data for the stacked bar chart
save_json(age_event_freq, "age_event_freq_stacked.json")

print("Stacked bar chart JSON file has been saved to the wwwroot/data folder.")
