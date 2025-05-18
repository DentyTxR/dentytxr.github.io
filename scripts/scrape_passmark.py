import requests
from bs4 import BeautifulSoup
import json

def scrape_table(url, table_id):
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                      "AppleWebKit/537.36 (KHTML, like Gecko) "
                      "Chrome/113.0.0.0 Safari/537.36"
    }
    response = requests.get(url, headers=headers)
    if not response.ok:
        raise Exception(f"Failed to fetch {url}: {response.status_code}")

    soup = BeautifulSoup(response.text, "html.parser")
    table = soup.find("table", id=table_id)
    if not table:
        raise Exception(f"Table with id '{table_id}' not found")

    data = {}
    for row in table.find_all("tr")[1:]:  # skip header
        cols = row.find_all("td")
        if len(cols) < 2:
            continue
        name = cols[0].get_text(strip=True)
        score = cols[1].get_text(strip=True).replace(",", "")
        try:
            data[name] = int(score)
        except ValueError:
            continue
    return data

def main():
    cpu_url = "https://www.cpubenchmark.net/cpu_list.php"
    gpu_url = "https://www.videocardbenchmark.net/gpu_list.php"

    cpu_data = scrape_table(cpu_url, "cputable")
    gpu_data = scrape_table(gpu_url, "cputable")  # both use "cputable"

    combined = {"cpu": cpu_data, "gpu": gpu_data}
    with open("passmark-data.json", "w") as f:
        json.dump(combined, f, indent=2)
    print("Saved benchmark data to passmark-data.json")

if __name__ == "__main__":
    main()
