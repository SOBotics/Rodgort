import { Observable, Observer } from 'rxjs';
import { share } from 'rxjs/operators';

export class WebsocketHelper<T> {
    public Observable: Observable<T>;
    constructor(url: string, autoReconnect = true) {
        this.Observable = Observable.create((obs: Observer<T>) => {
            const createWebsocket = () => {
                const websocket = new WebSocket(url);
                websocket.onmessage = event => {
                    if (event.data === 'ping') {
                        websocket.send('pong');
                    } else {
                        const data = JSON.parse(event.data) as T;
                        obs.next(data);
                    }
                };

                websocket.onclose = () => {
                    console.warn('Websocket for ' + url + ' closed.');
                    if (autoReconnect) {
                        console.log('Restarting websocket for ' + url);
                        setTimeout(() => {
                            createWebsocket();
                        }, 5000);
                    }
                };
            };
            createWebsocket();

            return () => { };
        }).pipe(share());
    }
}
