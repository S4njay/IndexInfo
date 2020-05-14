import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'IndexInfo'; 
  hide = true;
  mouseLeave() {
    this.hide = true;
  }
  mouseEnter() {
    this.hide = false;
  }
}
