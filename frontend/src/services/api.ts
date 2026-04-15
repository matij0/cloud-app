import axios from "axios";

const api = axios.create({
  baseURL: 'https://cloud-task-manager-api-96687-dcffgwezeggsgrh8.polandcentral-01.azurewebsites.net/api'
});

export default api;