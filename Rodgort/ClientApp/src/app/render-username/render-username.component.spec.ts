import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RenderUsernameComponent } from './render-username.component';

describe('RenderUsernameComponent', () => {
  let component: RenderUsernameComponent;
  let fixture: ComponentFixture<RenderUsernameComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RenderUsernameComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RenderUsernameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
