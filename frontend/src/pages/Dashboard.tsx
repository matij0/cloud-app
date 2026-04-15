import React, { useEffect, useState } from 'react';
import api from '../services/api';

interface CloudTask {
  id: number;
  name: string;
  isCompleted: boolean;
}

const Dashboard = () => {
  const [items, setItems] = useState<CloudTask[]>([]);
  const [error, setError] = useState("");
  const [newTaskName, setNewTaskName] = useState(""); 

  const fetchTasks = () => {
    api.get('/tasks')
      .then((res: any) => setItems(res.data))
      .catch((err: any) => {
        console.error("Błąd API:", err);
        setError("Błąd połączenia z API.");
      });
  };

  useEffect(() => {
    fetchTasks();
  }, []);

  // --- NOWA FUNKCJA: Usuwanie zadania ---
  const handleDelete = async (id: number) => {
    try {
      await api.delete(`/tasks/${id}`);
      // Usuwamy lokalnie z listy, żeby nie czekać na odświeżenie z bazy
      setItems(items.filter(item => item.id !== id));
    } catch (err) {
      setError("Nie udało się usunąć zadania.");
    }
  };

  // --- NOWA FUNKCJA: Zmiana statusu (Gotowe/W toku) ---
  const handleToggle = async (item: CloudTask) => {
    try {
      const updated = { ...item, isCompleted: !item.isCompleted };
      // Wysyłamy do backendu metodą PUT
      await api.put(`/tasks/${item.id}`, updated);
      // Aktualizujemy stan na ekranie
      setItems(items.map(t => t.id === item.id ? updated : t));
    } catch (err) {
      setError("Nie udało się zaktualizować zadania.");
    }
  };

  const handleAddTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTaskName.trim()) return;

    try {
      await api.post('/tasks', { name: newTaskName });
      setNewTaskName("");
      fetchTasks();
    } catch (err) {
      setError("Błąd podczas dodawania.");
    }
  };

  return (
    <div style={{ padding: '20px', textAlign: 'center', fontFamily: 'Arial, sans-serif' }}>
      <h1>☁️ Cloud App Dashboard</h1>

      {error && <div style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}

      <form onSubmit={handleAddTask} style={{ marginBottom: '30px' }}>
        <input 
          type="text" 
          placeholder="Wpisz nowe zadanie..." 
          value={newTaskName}
          onChange={(e) => setNewTaskName(e.target.value)}
          style={{ padding: '10px', width: '250px', borderRadius: '4px', border: '1px solid #ccc' }}
        />
        <button type="submit" style={{ marginLeft: '10px', padding: '10px 20px', cursor: 'pointer' }}>
          Dodaj
        </button>
      </form>

      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {items.map((item) => (
            <li key={item.id} style={{ 
              background: '#f8f9fa', margin: '10px', padding: '15px', borderRadius: '8px',
              borderLeft: item.isCompleted ? '5px solid #28a745' : '5px solid #6c757d',
              width: '400px', display: 'flex', justifyContent: 'space-between', alignItems: 'center'
            }}>
              <div>
                <input 
                  type="checkbox" 
                  checked={item.isCompleted} 
                  onChange={() => handleToggle(item)} 
                  style={{ marginRight: '10px' }}
                />
                <span style={{ textDecoration: item.isCompleted ? 'line-through' : 'none', color: '#333' }}>
                  {item.name}
                </span>
              </div>
              <button 
                onClick={() => handleDelete(item.id)}
                style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '18px' }}
              >
                🗑️
              </button>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default Dashboard;

 