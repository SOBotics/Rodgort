import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UnknownDeletionResolutionComponent } from './unknown-deletion-resolution.component';

describe('UnknownDeletionResolutionComponent', () => {
  let component: UnknownDeletionResolutionComponent;
  let fixture: ComponentFixture<UnknownDeletionResolutionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UnknownDeletionResolutionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UnknownDeletionResolutionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
