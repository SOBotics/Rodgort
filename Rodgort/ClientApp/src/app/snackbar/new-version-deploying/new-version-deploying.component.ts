import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';

@Component({
  selector: 'app-new-version-deploying',
  templateUrl: './new-version-deploying.component.html',
  styleUrls: ['./new-version-deploying.component.scss']
})
export class NewVersionDeployingComponent implements OnInit {

  constructor(private snackBar: MatSnackBar) { }

  ngOnInit() {

  }

  public dismissSnackbar() {
    this.snackBar.dismiss();
  }

}
