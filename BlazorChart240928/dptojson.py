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

# Grouping by Age to remove duplicates and aggregate values (mean or sum)
age_grouped = cust_info.groupby('Age').agg({
    'AverageAssets': 'mean',
    'AverageDebt': 'mean',
    'CardUsageValue': 'sum'
}).reset_index()

# 1. Average assets by age group
age_asset_avg = {
    "XAxis": age_grouped['Age'].tolist(),
    "YAxis": age_grouped['AverageAssets'].tolist()
}

# 2. Average debt by gender
gender_debt_avg = cust_info.groupby('Gender')['AverageDebt'].mean().reset_index()
gender_debt_avg_data = {
    "XAxis": gender_debt_avg['Gender'].tolist(),
    "YAxis": gender_debt_avg['AverageDebt'].tolist()
}

# 3. Card usage by region
region_card_usage_avg = cust_info.groupby('Region')['CardUsageValue'].sum().reset_index()
region_card_usage_data = {
    "XAxis": region_card_usage_avg['Region'].tolist(),
    "YAxis": region_card_usage_avg['CardUsageValue'].tolist()
}

# Merging customer info and app log data (based on SeqNo)
merged_data = pd.merge(cust_info, app_log, on='SeqNo', how='inner')

# 4. Event frequency by age group (Remove duplicates)
age_event_freq_data = merged_data.groupby(['Age', 'EventName']).size().reset_index(name='EventCount')
age_event_freq = {
    "XAxis": age_event_freq_data['Age'].tolist(),
    "YAxis": age_event_freq_data['EventCount'].tolist()
}

# 5. Event frequency by event type
event_freq_data = app_log['EventName'].value_counts().reset_index(name='EventCount')
event_freq_data.columns = ['EventName', 'EventCount']
event_freq = {
    "XAxis": event_freq_data['EventName'].tolist(),
    "YAxis": event_freq_data['EventCount'].tolist()
}

# 6. Event frequency by time of day
app_log['EventHour'] = pd.to_datetime(app_log['EventTime'], format='%H:%M:%S').dt.hour
hourly_events_data = app_log.groupby('EventHour').size().reset_index(name='EventCount')
hourly_events = {
    "XAxis": hourly_events_data['EventHour'].tolist(),
    "YAxis": hourly_events_data['EventCount'].tolist()
}

# Additional marketing suggestion data
# 7. Average card usage by age group
age_card_usage_avg = age_grouped[['Age', 'CardUsageValue']]
age_card_usage = {
    "XAxis": age_card_usage_avg['Age'].tolist(),
    "YAxis": age_card_usage_avg['CardUsageValue'].tolist()
}

# Path to save the JSON files
output_dir = "/home/dubukimch/blazor/wwwroot/data"
os.makedirs(output_dir, exist_ok=True)

# Function to save data as JSON
def save_json(data, filename):
    with open(os.path.join(output_dir, filename), 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

save_json(age_asset_avg, "age_asset_avg.json")
save_json(gender_debt_avg_data, "gender_debt_avg.json")
save_json(region_card_usage_data, "region_card_usage.json")
save_json(age_event_freq, "age_event_freq.json")
save_json(event_freq, "event_freq.json")
save_json(hourly_events, "hourly_events.json")
save_json(age_card_usage, "age_card_usage.json")

print("JSON files have been saved to the wwwroot/data folder.")
