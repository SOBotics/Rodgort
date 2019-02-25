import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TrackedBurnsComponent } from './tracked-burns.component';

describe('TrackedBurnsComponent', () => {
  let component: TrackedBurnsComponent;
  let fixture: ComponentFixture<TrackedBurnsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TrackedBurnsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TrackedBurnsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
