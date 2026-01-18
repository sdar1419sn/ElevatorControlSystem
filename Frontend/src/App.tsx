import { useState, useEffect } from 'react';
import axios from 'axios';

interface Elevator {
    id: number;
    currentFloor: number;
    direction: 'Up' | 'Down' | 'Idle';
    passengers: number[]; // destination floors
}

const FLOORS = Array.from({ length: 10 }, (_, i) => 10 - i); // 10 → 1

function App() {
    const [elevators, setElevators] = useState<Elevator[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchStatus = async () => {
            try {
                const res = await axios.get('http://localhost:5000/api/Elevator/status');
                const data = Array.isArray(res.data) ? res.data : [];
                setElevators(data);
                setLoading(false);
                setError(null);
            } catch (err: any) {
                setError('Cannot connect to backend. Make sure API is running on port 5000.');
                setLoading(false);
            }
        };

        fetchStatus();
        const interval = setInterval(fetchStatus, 3000);
        return () => clearInterval(interval);
    }, []);

    return (
        <div style={{ fontFamily: 'system-ui, sans-serif', padding: '1.5rem', background: '#f0f2f5', minHeight: '100vh' }}>
            <h1 style={{ textAlign: 'center', color: '#1a237e', marginBottom: '2rem' }}>
                Elevator Control System – Live Monitoring
            </h1>

            {error && (
                <div style={{ background: '#ffebee', color: '#c62828', padding: '1rem', borderRadius: '8px', marginBottom: '1.5rem', textAlign: 'center' }}>
                    {error}
                </div>
            )}

            {loading ? (
                <p style={{ textAlign: 'center', fontSize: '1.3rem', color: '#555' }}>Loading live status...</p>
            ) : elevators.length === 0 ? (
                <p style={{ textAlign: 'center', color: '#777' }}>Waiting for simulation data...</p>
            ) : (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))', gap: '2rem' }}>
                    {elevators.map((el) => (
                        <div key={el.id} style={{
                            border: '2px solid #555',
                            borderRadius: '12px',
                            overflow: 'hidden',
                            background: '#fff',
                            boxShadow: '0 6px 20px rgba(0,0,0,0.12)',
                            transition: 'transform 0.3s'
                        }}>
                            {/* Header */}
                            <div style={{
                                background: '#1a237e',
                                color: 'white',
                                padding: '0.8rem',
                                textAlign: 'center',
                                fontWeight: 'bold',
                                fontSize: '1.2rem'
                            }}>
                                Elevator {el.id} {el.direction === 'Up' ? '↑' : el.direction === 'Down' ? '↓' : '—'}
                            </div>

                            {/* Shaft */}
                            <div style={{
                                position: 'relative',
                                height: '420px',
                                background: 'linear-gradient(to bottom, #e3f2fd, #bbdefb)',
                                borderBottom: '4px solid #444'
                            }}>
                                {/* Floor lines & labels */}
                                {FLOORS.map((f) => (
                                    <div key={f} style={{
                                        position: 'absolute',
                                        bottom: `${(f - 1) * 42}px`,
                                        left: 0,
                                        right: 0,
                                        height: '42px',
                                        borderBottom: '1px solid #aaa',
                                        display: 'flex',
                                        alignItems: 'center',
                                        paddingLeft: '12px',
                                        fontSize: '0.95rem',
                                        color: '#333'
                                    }}>
                                        Floor {f}
                                    </div>
                                ))}

                                {/* Elevator car */}
                                <div style={{
                                    position: 'absolute',
                                    bottom: `${(el.currentFloor - 1) * 42}px`,
                                    left: '15%',
                                    width: '70%',
                                    height: '42px',
                                    background: el.passengers.length > 0 ? '#4caf50' : '#757575',
                                    border: '3px solid #333',
                                    borderRadius: '8px',
                                    transition: 'bottom 2.5s ease-in-out',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    color: 'white',
                                    fontWeight: 'bold',
                                    fontSize: '1.4rem',
                                    boxShadow: 'inset 0 2px 10px rgba(0,0,0,0.4)'
                                }}>
                                    {el.passengers.length} 👥
                                </div>
                            </div>

                            {/* Footer info */}
                            <div style={{ padding: '1rem', textAlign: 'center', background: '#f5f5f5' }}>
                                <strong>Passengers:</strong> {el.passengers.length}
                                {el.passengers.length > 0 && (
                                    <div style={{ marginTop: '0.5rem', fontSize: '0.95rem', color: '#555' }}>
                                        Going to: {el.passengers.join(', ')}
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default App;