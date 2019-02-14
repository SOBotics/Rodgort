import { Observable, Observer } from 'rxjs';
import { share } from 'rxjs/operators';

export class WebsocketHelper<T> {
    public Observable: Observable<T>;
    constructor(url: string) {
        this.Observable = Observable.create((obs: Observer<T>) => {
            const createWebsocket = () => {
                console.log('Started new websocket for ' + url);
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
                    createWebsocket();
                };
            };
            createWebsocket();

            return () => { };
        }).pipe(share());
    }
}
