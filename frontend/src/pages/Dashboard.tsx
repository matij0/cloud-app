import React, { useEffect, useState } from 'react';
import api from '../services/api';

// Definicja typu dla zadań (zgodna z modelem w .NET i DTO)
interface CloudTask {
  id: number;
  name: string;
  isCompleted: boolean;
}

const Dashboard = () => {
  // Stan dla listy zadań, błędów oraz nazwy nowego zadania
  const [items, setItems] = useState<CloudTask[]>([]);
  const [error, setError] = useState("");
  const [newTaskName, setNewTaskName] = useState(""); 

  // 1. Funkcja pobierająca zadania - zdefiniowana osobno, by móc ją wywołać wielokrotnie
  const fetchTasks = () => {
    api.get('/tasks')
      .then((res: any) => {
        setItems(res.data);
      })
      .catch((err: any) => {
        console.error("Szczegóły błędu:", err);
        setError("Błąd połączenia z API. Sprawdź, czy backend działa.");
      });
  };

  // 2. useEffect uruchamia pobieranie danych przy pierwszym wejściu na stronę
  useEffect(() => {
    fetchTasks();
  }, []);

  // 3. Funkcja obsługująca wysyłanie formularza (Dodawanie zadania)
  const handleAddTask = async (e: React.FormEvent) => {
    e.preventDefault(); // Zapobiega przeładowaniu strony po kliknięciu przycisku

    if (!newTaskName.trim()) return; // Nie wysyłaj, jeśli pole jest puste

    try {
      // Wykorzystujemy nasz kontrakt API - wysyłamy tylko to, czego oczekuje TaskCreateDto
      await api.post('/tasks', {
        name: newTaskName
      });

      setNewTaskName(""); // Czyścimy pole tekstowe dla wygody użytkownika
      fetchTasks();       // Pobieramy aktualną listę z bazy, by zobaczyć nowe zadanie
    } catch (err) {
      console.error("Błąd podczas dodawania zadania:", err);
      setError("Nie udało się dodać zadania. Spróbuj ponownie.");
    }
  };

  return (
    <div style={{ padding: '20px', textAlign: 'center', fontFamily: 'Arial, sans-serif' }}>
      <h1>☁️ Cloud App Dashboard</h1>

      {/* Komunikat o błędzie */}
      {error && (
        <div style={{ background: '#fff3cd', color: '#856404', padding: '10px', borderRadius: '5px', margin: '20px auto', maxWidth: '400px' }}>
          {error}
        </div>
      )}

      {/* FORMULARZ DODAWANIA ZADAŃ (Produkcyjne UI) */}
      <form onSubmit={handleAddTask} style={{ marginBottom: '30px' }}>
        <input 
          type="text" 
          placeholder="Wpisz nowe zadanie..." 
          value={newTaskName}
          onChange={(e) => setNewTaskName(e.target.value)}
          style={{ padding: '10px', width: '250px', borderRadius: '4px', border: '1px solid #ccc' }}
        />
        <button type="submit" style={{ 
          marginLeft: '10px', 
          padding: '10px 20px', 
          backgroundColor: '#007bff', 
          color: 'white', 
          border: 'none', 
          borderRadius: '4px', 
          cursor: 'pointer' 
        }}>
          Dodaj Zadanie
        </button>
      </form>

      {/* LISTA ZADAŃ */}
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        {items.length === 0 && !error && <p>Brak zadań. Czas coś zaplanować!</p>}

        <ul style={{ listStyle: 'none', padding: 0 }}>
          {items.map((item) => (
            <li key={item.id} style={{ 
              background: '#f8f9fa', 
              margin: '5px', 
              padding: '10px 20px', 
              borderRadius: '8px',
              borderLeft: item.isCompleted ? '5px solid #28a745' : '5px solid #6c757d',
              width: '350px',
              textAlign: 'left',
              boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
            }}>
              <strong>{item.name}</strong> {item.isCompleted ? '✅' : '⏳'}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default Dashboard;
 