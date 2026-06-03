const dbName = "SocialGasyDB";
let db;

// 1. Fanokafana ny database
const request = indexedDB.open(dbName, 1);

request.onupgradeneeded = (e) => {
    let db = e.target.result;
    if (!db.objectStoreNames.contains("pendingData")) {
        db.createObjectStore("pendingData", { keyPath: "id" });
    }
};

request.onsuccess = (e) => {
    db = e.target.result;
    console.log("Database SocialGasy vonona!");
    // Fanamarinana ho azy raha vao tafiditra ny pejy
    uploadPendingData();
};

// 2. Fomba fanatobiana data (Ity no antsoinao ao amin'ny formulaire)
function storeOffline(type, data) {
    if (!db) return;
    const tx = db.transaction("pendingData", "readwrite");
    const store = tx.objectStore("pendingData");
    const entry = {
        id: Date.now(),
        type: type,
        data: data
    };
    store.add(entry);
    console.log("Data voatahiry an-toerana (offline):", type);
}

// 3. Fomba famakiana data
function getAllPending() {
    return new Promise((resolve) => {
        if (!db) resolve([]);
        const tx = db.transaction("pendingData", "readonly");
        const store = tx.objectStore("pendingData");
        const req = store.getAll();
        req.onsuccess = () => resolve(req.result);
    });
}

// 4. Fomba fanadiovana data
function clearPendingData() {
    if (!db) return;
    const tx = db.transaction("pendingData", "readwrite");
    tx.objectStore("pendingData").clear();
}

// 5. Fomba fanaterana data (Sync function)
async function uploadPendingData() {
    if (!navigator.onLine || !db) return;

    const data = await getAllPending();
    if (data.length === 0) return;

    const payload = {
        Households: data.filter(i => i.type === 'household').map(i => i.data),
        Citizens: data.filter(i => i.type === 'citizen').map(i => i.data)
    };

    try {
        const res = await fetch('/api/sync/upload', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            clearPendingData();
            console.log("Sync vita tsara!");
        }
    } catch (err) {
        console.error("Tsy nahomby ny sync:", err);
    }
}

// 6. Listener ho an'ny Internet
window.addEventListener('online', uploadPendingData);