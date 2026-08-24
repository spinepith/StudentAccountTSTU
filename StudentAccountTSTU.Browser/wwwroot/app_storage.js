// FILE STORAGE
export async function getItem(key) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readonly');
        const store = tx.objectStore('files');
        const request = store.get(key);
        request.onsuccess = () => {
            if (request.result) {
                if (typeof request.result === 'object' && 'data' in request.result) {
                    resolve(request.result.data);
                } else {
                    resolve(request.result);
                }
            } else {
                resolve(null);
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function setItem(key, value) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readwrite');
        const store = tx.objectStore('files');
        const record = { data: value, lastModified: Date.now() };
        const request = store.put(record, key);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
}

export async function checkExists(key) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readonly');
        const store = tx.objectStore('files');
        const request = store.count(key);
        request.onsuccess = () => resolve(request.result > 0);
        request.onerror = () => reject(request.error);
    });
}

export async function getLastModified(key) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readonly');
        const store = tx.objectStore('files');
        const request = store.get(key);
        request.onsuccess = () => {
            if (request.result && typeof request.result === 'object' && request.result.lastModified) {
                resolve(request.result.lastModified);
            } else {
                resolve(-1);
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function removeFile(key) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readwrite');
        const store = tx.objectStore('files');
        const request = store.delete(key);
        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error);
    });
}

export async function removeDirectory(prefix) {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readwrite');
        const store = tx.objectStore('files');
        const request = store.openCursor();
        request.onsuccess = (e) => {
            const cursor = e.target.result;
            if (cursor) {
                const keyStr = cursor.key.replace(/\\/g, '/');
                const pfxStr = prefix.replace(/\\/g, '/');
                if (keyStr.startsWith(pfxStr + '/') || keyStr === pfxStr) {
                    cursor.delete();
                }
                cursor.continue();
            } else {
                resolve();
            }
        };
        request.onerror = () => reject(request.error);
    });
}

export async function getAllKeys() {
    const db = await getDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction('files', 'readonly');
        const store = tx.objectStore('files');
        const request = store.getAllKeys();
        request.onsuccess = () => {
            if (request.result) {
                resolve(request.result.join(','));
            } else {
                resolve("");
            }
        };
        request.onerror = () => reject(request.error);
    });
}

function getDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('StudentAccountTSTU', 1);
        request.onupgradeneeded = (e) => {
            const db = e.target.result;
            if (!db.objectStoreNames.contains('files')) {
                db.createObjectStore('files');
            }
        };
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

// SETTINGS
export function getLocalStorageItem(key) {
    return localStorage.getItem(key);
}

export function setLocalStorageItem(key, value) {
    localStorage.setItem(key, value);
}
