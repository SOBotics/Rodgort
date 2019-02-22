import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ZombiesComponent } from './zombies.component';

describe('ZombiesComponent', () => {
  let component: ZombiesComponent;
  let fixture: ComponentFixture<ZombiesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ZombiesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ZombiesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
