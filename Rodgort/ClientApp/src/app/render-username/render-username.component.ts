import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-render-username',
  templateUrl: './render-username.component.html',
  styleUrls: ['./render-username.component.scss']
})
export class RenderUsernameComponent implements OnInit {

  @Input()
  public user: {
    userId: number;
    displayName: string;
    isModerator: boolean;
    reputation: number;
  };

  @Input()
  public warnEditPrivileges = false;

  @Input()
  public hyperlinkName = true;

  constructor() { }

  ngOnInit() {
  }

}
