import { Observable, Observer } from 'rxjs';
import { share } from 'rxjs/operators';
import { environment } from '../environments/environment';

export class WebsocketHelper<T> {
    public Observable: Observable<T>;
    constructor(url: string, autoReconnect = true) {
        const fixedUrl = `${environment.websocketProtocol}://${location.host}/${url}`;
        this.Observable = Observable.create((obs: Observer<T>) => {
            const createWebsocket = () => {
                const websocket = new WebSocket(fixedUrl);
                websocket.onmessage = event => {
                    if (event.data === 'ping') {
                        websocket.send('pong');
                    } else {
                        const data = JSON.parse(event.data) as T;
                        obs.next(data);
                    }
                };

                websocket.onclose = () => {
                    console.warn('Websocket for ' + fixedUrl + ' closed.');
                    if (autoReconnect) {
                        console.log('Restarting websocket for ' + fixedUrl);
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
