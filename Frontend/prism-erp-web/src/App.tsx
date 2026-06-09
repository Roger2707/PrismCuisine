import { useAppSelector } from './app/hooks';
import './App.css';

function App() {
  const apiBaseUrl = useAppSelector((state) => state.app.apiBaseUrl);

  return (
    <main className="app">
      <header>
        <h1>Prism Cuisine</h1>
        <p>React + Redux Toolkit</p>
      </header>
      <section className="card">
        <p>API base URL: {apiBaseUrl}</p>
      </section>
    </main>
  );
}

export default App;
